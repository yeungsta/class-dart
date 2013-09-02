using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Data;
using System.Data.Entity;
using ClassDart.Models;

namespace ClassDart.Controllers
{
    public class HomeController : Controller
    {
        private ClassDartDBContext db = new ClassDartDBContext();

        public ActionResult Index()
        {
            MembershipUser currentUser = null;

            try
            {
                currentUser = Membership.GetUser(User.Identity.Name, true /* userIsOnline */);
            }
            catch (Exception e)
            {
                //Utilities.LogAppError("Could not retrieve user account.", e);
                return HttpNotFound();
            }

            if (currentUser != null)
            {
                IOrderedQueryable<Class> classes = Utilities.GetInstructorClassesFromEmail(db, currentUser.Email);

                if (classes != null)
                {
                    TempData["ClassList"] = classes.ToList();
                }
            }

            return View();
        }

        //
        // POST: /Account/Index
        // Handle form posts

        [HttpPost]
        public ActionResult Index(HomeViewModel model, string returnUrl, string submitButton)
        {
            if (submitButton == "Sign In")
            {
                if (ModelState.IsValid)
                {
                    if (Membership.ValidateUser(model.SignInViewModel.UserName, model.SignInViewModel.Password))
                    {
                        FormsAuthentication.SetAuthCookie(model.SignInViewModel.UserName, model.SignInViewModel.RememberMe);
                        if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                            && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                        {
                            return Redirect(returnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("Sign In Error", "The user name or password provided is incorrect.");
                    }
                }

                // If we got this far, something failed, redisplay form
                return View(model);                
            }
            else if (submitButton == "Sign Up Free")
            {
                if (ModelState.IsValid)
                {
                    // Attempt to create the user account
                    MembershipCreateStatus createStatus;
                    MembershipUser creationInfo = Membership.CreateUser(model.NewUserViewModel.UserName, model.NewUserViewModel.Password, model.NewUserViewModel.UserName, null, null, true, null, out createStatus);

                    if (createStatus == MembershipCreateStatus.Success)
                    {
                        FormsAuthentication.SetAuthCookie(model.NewUserViewModel.UserName, false /* createPersistentCookie */);

                        //create Instructor profile
                        Instructor instructorObj = new Instructor();
                        instructorObj.User = model.NewUserViewModel.UserName;
                        instructorObj.FirstName = model.NewUserViewModel.FirstName;
                        instructorObj.LastName = model.NewUserViewModel.LastName;
                        instructorObj.Handle = Utilities.CreateUniqueInstructorHandle(db, model.NewUserViewModel.FirstName, model.NewUserViewModel.LastName);
                        db.Instructors.Add(instructorObj);
                        db.SaveChanges();

                        /*
                        //Email welcome message to user
                        try //TODO: remove try/catch when using real SMTP server in production
                        {
                            new MailController().SendSignUpEmail(model.Email).Deliver();

                            //send to Support email, too
                            new MailController().NewUserNoticeEmail(model.Email).Deliver();
                        }
                        catch
                        {
                        }

                        if (!string.IsNullOrEmpty(model.ReturnAction) && !string.IsNullOrEmpty(model.ReturnController))
                        {
                            return RedirectToAction(model.ReturnAction, model.ReturnController, new { id = model.ReturnMenuId });
                        }
                        else
                        {
                            //send to Dashboard by default
                            return RedirectToAction("Index", "Dashboard");
                        }
                        */

                        //return, but signed in
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError("New User Error", Utilities.ErrorCodeToString(createStatus));
                    }
                }

                // If we got this far, something failed, redisplay form
                return View(model);
            }
            else if (submitButton == "Create")
            {
                if ((ModelState.IsValid) && (!string.IsNullOrEmpty(model.CreateClassViewModel.ClassName)))
                {
                    List<Class> tempClassList = new List<Class>();

                    //get all existing classes
                    MembershipUser currentUser = null;

                    try
                    {
                        currentUser = Membership.GetUser(User.Identity.Name, true /* userIsOnline */);
                    }
                    catch (Exception e)
                    {
                        //Utilities.LogAppError("Could not retrieve user account.", e);
                        return HttpNotFound();
                    }

                    if (currentUser != null)
                    {
                        IOrderedQueryable<Class> classes = Utilities.GetInstructorClassesFromEmail(db, currentUser.Email);

                        if (classes != null)
                        {
                            tempClassList = classes.ToList();
                        }
                    }

                    //check if class name already exists
                    if (!tempClassList.Any(x => x.Name.ToLower() == model.CreateClassViewModel.ClassName.ToLower()))
                    {
                        //add new class
                        Class newClass = new Class();
                        newClass.Name = model.CreateClassViewModel.ClassName;
                        newClass.Instructor = currentUser.Email;
                        newClass.Template = "Licorice";

                        //get Instructor profile
                        Instructor instructorObj;

                        Utilities.GetInstructorObjFromEmail(db, currentUser.Email, out instructorObj);

                        newClass.Url = Utilities.CreateClassDartUrl(instructorObj.Handle, model.CreateClassViewModel.ClassName);

                        db.Classes.Add(newClass);
                        db.SaveChanges();

                        //create classdart page and create/update instructor page
                        Utilities.SaveToClassDartAndInstructorPage(newClass, instructorObj);

                        tempClassList.Add(newClass);
                        TempData["ClassList"] = tempClassList;
                    }
                    else
                    {
                        return RedirectToAction("DuplicateClassName");
                    }
                }
             
                return RedirectToAction("Index", "Home");
            }
            else
            {
                //search classes

                // If we got this far, something failed, redisplay form
                return View(model);
            }
        }

        //
        // POST: /Home/DeleteClassConfirmed/1
        [Authorize]
        public ActionResult DeleteClassConfirmed(int id)
        {
            Class classObj = db.Classes.Find(id);

            if (classObj == null)
            {
                return HttpNotFound();
            }

            Instructor instructorObj;

            if (!Utilities.GetInstructorObjFromEmail(db, classObj.Instructor, out instructorObj))
            {
                return HttpNotFound();
            }

            //remove class from DB
            db.Classes.Remove(classObj);
            db.SaveChanges();

            //re-create instructor page since a class was deleted
            Utilities.SaveToInstructorPage(classObj, instructorObj);

#if UseAmazonS3
            //remove class page from S3
            string classPath = classObj.Url + "/index.html";

            if (Utilities.IsObjectExistS3(classPath))
            {
                Utilities.RemoveObjectFromS3(classPath);
            }
#endif
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Home/DuplicateClassName

        [Authorize]
        public ActionResult DuplicateClassName()
        {
            return View();
        }
    }
}
