using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using ClassDart.Models;
using ClassDart.Composer;

namespace ClassDart.Controllers
{
    public class EditorController : Controller
    {
        private ClassDartDBContext db = new ClassDartDBContext();

        //
        // GET: /Editor/
        [Authorize]
        public ActionResult Index(int id = 0)
        {
            Class classObj;

            if (!Utilities.GetClassObjFromId(db, id, out classObj))
            {
                return HttpNotFound();
            }

            Instructor instructorObj;

            if (!Utilities.GetInstructorObjFromEmail(db, classObj.Instructor, out instructorObj))
            {
                return HttpNotFound();
            }

            EditorViewModel editorViewData = new EditorViewModel();
            //data
            editorViewData.classObj = classObj;
            editorViewData.instructorObj = instructorObj;
            editorViewData.announcements = V1.DeserializeAnnouncements(classObj.Announcements);
            editorViewData.assignments = V1.DeserializeAssignments(classObj.Assignments);
            editorViewData.subscribers = V1.DeserializeSubscribers(classObj.Subscribers);
            editorViewData.fullUrl = Utilities.GetFullUrl(classObj.Url);
            //labels
            editorViewData.AnnouncementsLbl = Constants.AnnouncementsLbl;
            editorViewData.AssignmentsLbl = Constants.AssignmentsLbl;
            editorViewData.InstructorLbl = Constants.InstructorLbl;

            return View(editorViewData);
        }

        //
        // POST: /Editor/Index
        // Handle form posts
        [Authorize]
        [HttpPost]
        public ActionResult Index(EditorViewModel model, int id, string returnUrl, string submitButton)
        {
            Class classObj;

            if (!Utilities.GetClassObjFromId(db, id, out classObj))
            {
                return HttpNotFound();
            } 

            if (submitButton == "Create Announcement")
            {
                if (ModelState.IsValid)
                {
                    IList <Announcement> currentAnnouncements = V1.DeserializeAnnouncements(classObj.Announcements);
                    
                    Announcement newAnnouncement = new Announcement();
                    newAnnouncement.Text = model.AnnouncementField;
                    newAnnouncement.UpdateDateTime = DateTime.Now;
                    currentAnnouncements.Add(newAnnouncement);

                    classObj.Announcements = V1.SerializeAnnouncements(currentAnnouncements);

                    //save classObj in DB
                    Utilities.SaveToDb(db, classObj);
                    Utilities.SaveToClassDart(classObj, null);

                    //Send notif email to subscribers
                    Utilities.NotifySubscribers(classObj);
                }

                return RedirectToAction("Index", "Editor");
            }
            else if (submitButton == "Create Assignment")
            {
                if (ModelState.IsValid)
                {
                    IList<Assignment> currentAssignments = V1.DeserializeAssignments(classObj.Assignments);

                    Assignment newAssignment = new Assignment();
                    newAssignment.Title = model.AssignmentTitleField;
                    //newAssignment.DueDateTime = 
                    newAssignment.Text = model.AssignmentTextField;
                    newAssignment.UpdateDateTime = DateTime.Now;
                    currentAssignments.Add(newAssignment);

                    classObj.Assignments = V1.SerializeAssignments(currentAssignments);

                    //save classObj in DB
                    Utilities.SaveToDb(db, classObj);
                    Utilities.SaveToClassDart(classObj, null);

                    //Send notif email to subscribers
                    Utilities.NotifySubscribers(classObj);
                }

                //return to assignments page
                return new RedirectResult(Url.Action("Index") + "#assignments");
            }
            else if (submitButton == "Save Name")
            {
                Instructor instructorObj;

                if (!Utilities.GetInstructorObjFromEmail(db, classObj.Instructor, out instructorObj))
                {
                    return HttpNotFound();
                }

                instructorObj.Prefix = model.instructorObj.Prefix;
                instructorObj.FirstName = model.instructorObj.FirstName;
                instructorObj.MiddleInitial = model.instructorObj.MiddleInitial;
                instructorObj.LastName = model.instructorObj.LastName;

                //save intructorObj in DB
                Utilities.SaveToDb(db, instructorObj);

                //re-create classdarts for all classes
                foreach (Class item in Utilities.GetInstructorClassesFromEmail(db, classObj.Instructor))
                {
                    Utilities.SaveToClassDart(item, instructorObj);
                }

                //re-create instructor page since name changed
                Utilities.SaveToInstructorPage(classObj, instructorObj);
                
                //return to instructor page
                return new RedirectResult(Url.Action("Index") + "#instructor");
            }
            else if (submitButton == "Save Bio")
            {
                Instructor instructorObj;

                if (!Utilities.GetInstructorObjFromEmail(db, classObj.Instructor, out instructorObj))
                {
                    return HttpNotFound();
                }

                instructorObj.Bio = model.instructorObj.Bio;

                //save intructorObj in DB
                Utilities.SaveToDb(db, instructorObj);

                //re-create classdarts for all classes
                foreach (Class item in Utilities.GetInstructorClassesFromEmail(db, classObj.Instructor))
                {
                    Utilities.SaveToClassDart(item, instructorObj);
                }

                //re-create instructor page since bio changed
                Utilities.SaveToInstructorPage(classObj, instructorObj);

                //return to instructor page
                return new RedirectResult(Url.Action("Index") + "#instructor");
            }
            else if (submitButton == "Save Email")
            {
                Instructor instructorObj;

                if (!Utilities.GetInstructorObjFromEmail(db, classObj.Instructor, out instructorObj))
                {
                    return HttpNotFound();
                }

                instructorObj.Email = model.instructorObj.Email;

                //save intructorObj in DB
                Utilities.SaveToDb(db, instructorObj);

                //re-create classdarts for all classes
                foreach (Class item in Utilities.GetInstructorClassesFromEmail(db, classObj.Instructor))
                {
                    Utilities.SaveToClassDart(item, instructorObj);
                }

                //re-create instructor page since info changed
                Utilities.SaveToInstructorPage(classObj, instructorObj);

                //return to instructor page
                return new RedirectResult(Url.Action("Index") + "#instructor");
            }
            else if (submitButton == "Save Phone")
            {
                Instructor instructorObj;

                if (!Utilities.GetInstructorObjFromEmail(db, classObj.Instructor, out instructorObj))
                {
                    return HttpNotFound();
                }

                instructorObj.Phone = model.instructorObj.Phone;

                //save intructorObj in DB
                Utilities.SaveToDb(db, instructorObj);

                //re-create classdarts for all classes
                foreach (Class item in Utilities.GetInstructorClassesFromEmail(db, classObj.Instructor))
                {
                    Utilities.SaveToClassDart(item, instructorObj);
                }

                //re-create instructor page since info changed
                Utilities.SaveToInstructorPage(classObj, instructorObj);

                //return to instructor page
                return new RedirectResult(Url.Action("Index") + "#instructor");
            }
            else if (submitButton == "Save Facebook")
            {
                Instructor instructorObj;

                if (!Utilities.GetInstructorObjFromEmail(db, classObj.Instructor, out instructorObj))
                {
                    return HttpNotFound();
                }

                instructorObj.Facebook = Utilities.CleanUrl(model.instructorObj.Facebook);

                //save intructorObj in DB
                Utilities.SaveToDb(db, instructorObj);

                //re-create classdarts for all classes
                foreach (Class item in Utilities.GetInstructorClassesFromEmail(db, classObj.Instructor))
                {
                    Utilities.SaveToClassDart(item, instructorObj);
                }

                //re-create instructor page since info changed
                Utilities.SaveToInstructorPage(classObj, instructorObj);

                //return to instructor page
                return new RedirectResult(Url.Action("Index") + "#instructor");
            }
            else if (submitButton == "Save LinkedIn")
            {
                Instructor instructorObj;

                if (!Utilities.GetInstructorObjFromEmail(db, classObj.Instructor, out instructorObj))
                {
                    return HttpNotFound();
                }

                instructorObj.LinkedIn = Utilities.CleanUrl(model.instructorObj.LinkedIn);

                //save intructorObj in DB
                Utilities.SaveToDb(db, instructorObj);

                //re-create classdarts for all classes
                foreach (Class item in Utilities.GetInstructorClassesFromEmail(db, classObj.Instructor))
                {
                    Utilities.SaveToClassDart(item, instructorObj);
                }

                //re-create instructor page since info changed
                Utilities.SaveToInstructorPage(classObj, instructorObj);

                //return to instructor page
                return new RedirectResult(Url.Action("Index") + "#instructor");
            }
            else if (submitButton == "Save Twitter")
            {
                Instructor instructorObj;

                if (!Utilities.GetInstructorObjFromEmail(db, classObj.Instructor, out instructorObj))
                {
                    return HttpNotFound();
                }

                instructorObj.Twitter = Utilities.CleanTwitter(model.instructorObj.Twitter);

                //save intructorObj in DB
                Utilities.SaveToDb(db, instructorObj);

                //re-create classdarts for all classes
                foreach (Class item in Utilities.GetInstructorClassesFromEmail(db, classObj.Instructor))
                {
                    Utilities.SaveToClassDart(item, instructorObj);
                }

                //re-create instructor page since info changed
                Utilities.SaveToInstructorPage(classObj, instructorObj);

                //return to instructor page
                return new RedirectResult(Url.Action("Index") + "#instructor");
            }
            else if (submitButton == "Save Website")
            {
                Instructor instructorObj;

                if (!Utilities.GetInstructorObjFromEmail(db, classObj.Instructor, out instructorObj))
                {
                    return HttpNotFound();
                }

                instructorObj.Website = Utilities.CleanUrl(model.instructorObj.Website);

                //save intructorObj in DB
                Utilities.SaveToDb(db, instructorObj);

                //re-create classdarts for all classes
                foreach (Class item in Utilities.GetInstructorClassesFromEmail(db, classObj.Instructor))
                {
                    Utilities.SaveToClassDart(item, instructorObj);
                }

                //re-create instructor page since info changed
                Utilities.SaveToInstructorPage(classObj, instructorObj);

                //return to instructor page
                return new RedirectResult(Url.Action("Index") + "#instructor");
            } 
            
            return RedirectToAction("Index", "Editor");
        }

        //
        // POST: /Editor/DeleteAnnouncementConfirmed/1
        [Authorize]
        public ActionResult DeleteAnnouncementConfirmed(int id, int index)
        {
            Class classObj = db.Classes.Find(id);

            if (classObj == null)
            {
                return HttpNotFound();
            }

            IList<Announcement> currentAnnouncements = V1.DeserializeAnnouncements(classObj.Announcements);

            currentAnnouncements.RemoveAt(index);

            classObj.Announcements = V1.SerializeAnnouncements(currentAnnouncements);

            //save classObj in DB
            db.Entry(classObj).State = EntityState.Modified;
            db.SaveChanges();

            Utilities.SaveToClassDart(classObj, null);

            return RedirectToAction("Index", "Editor", new { id = id });
        }

        //
        // POST: /Editor/DeleteAssignmentConfirmed/1
        [Authorize]
        public ActionResult DeleteAssignmentConfirmed(int id, int index)
        {
            Class classObj = db.Classes.Find(id);

            if (classObj == null)
            {
                return HttpNotFound();
            }

            IList<Assignment> currentAssignments = V1.DeserializeAssignments(classObj.Assignments);

            currentAssignments.RemoveAt(index);

            classObj.Assignments = V1.SerializeAssignments(currentAssignments);

            //save classObj in DB
            db.Entry(classObj).State = EntityState.Modified;
            db.SaveChanges();

            Utilities.SaveToClassDart(classObj, null);

            //return to assignments page
            return new RedirectResult(Url.Action("Index", new { id = id }) + "#assignments");
        }
    }
}
