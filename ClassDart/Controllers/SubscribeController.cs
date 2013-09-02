using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using ClassDart.Models;
using ClassDart.Composer;

namespace ClassDart.Controllers
{
    public class SubscribeController : Controller
    {
        private ClassDartDBContext db = new ClassDartDBContext();

        //
        // POST: /Subscribe/Index
        // Handle form posts
        [HttpPost]
        public ActionResult Index(string classInput, string emailInput, string returnUrl)
        {
            Class classObj;

            if (!Utilities.GetClassObjFromId(db, int.Parse(classInput), out classObj))
            {
                return HttpNotFound();
            }

            IList<Subscriber> currentSubscribers = V1.DeserializeSubscribers(classObj.Subscribers);

            string cleanedEmail = emailInput.Trim().ToLower();

            //first check if email is already subscribed
            if (!currentSubscribers.Any(x => x.Email == cleanedEmail))
            {
                Subscriber newSubscriber = new Subscriber();
                newSubscriber.Email = cleanedEmail;   //TODO: validate email format: "@"
                currentSubscribers.Add(newSubscriber);

                classObj.Subscribers = V1.SerializeSubscribers(currentSubscribers);

                //save classObj in DB
                Utilities.SaveToDb(db, classObj);

                //Send email to subscriber (on non-blocking background thread)
                new Thread(() =>
                {
                    try //TODO: remove try/catch when using real SMTP server in production
                    {
                        //synchronous
                        //new MailController().SendSubscribedEmail(emailInput, classObj.Name, classObj.ID).Deliver();
                        //asynchronous (but using DeliverAync doesn't work w/ a real SMTP server for some reason)
                        new MailControllerAsync().SendSubscribedEmailAsync(emailInput, classObj.Name, classObj.ID).Deliver();
                    }
                    catch
                    {
                    }
                }).Start();
            }
            else
            {
                return RedirectToAction("AlreadySubscribed");
            }

            //todo: add return URL button
            //return Redirect(returnUrl);
            return RedirectToAction("SubscribeSuccess");
        }

        //
        // Get: /Subscribe/Unsubscribe
        public ActionResult Unsubscribe(string emailInput, int id = 0)
        {
            Class classObj;

            if (!Utilities.GetClassObjFromId(db, id, out classObj))
            {
                return HttpNotFound();
            }

            IList<Subscriber> currentSubscribers = V1.DeserializeSubscribers(classObj.Subscribers);

            if (currentSubscribers.Any(x => x.Email == emailInput))
            {
                Subscriber removeSubscriber = currentSubscribers.First(x => x.Email == emailInput);

                currentSubscribers.Remove(removeSubscriber);

                classObj.Subscribers = V1.SerializeSubscribers(currentSubscribers);

                //save classObj in DB
                Utilities.SaveToDb(db, classObj);

                /*
                //Send email to unsubscriber (to unsubscribe from verified email)
                try //TODO: remove try/catch when using real SMTP server in production
                {
                    //new MailController().SendSubscribedEmail(emailInput, classObj.Name).Deliver();
                }
                catch
                {
                }
                 */
            }
            else
            {
                //direct to page saying subscriber not found
                return RedirectToAction("SubscriberNotFound");
            }

            return RedirectToAction("UnsubscribeSuccess");
        }

        //
        // GET: /Subscription/SubscriberNotFound

        [Authorize]
        public ActionResult SubscriberNotFound()
        {
            return View();
        }

        //
        // GET: /Subscription/SubscribeSuccess

        [Authorize]
        public ActionResult SubscribeSuccess()
        {
            return View();
        }

        //
        // GET: /Subscription/UnsubscribeSuccess

        [Authorize]
        public ActionResult UnsubscribeSuccess()
        {
            return View();
        }

        //
        // GET: /Subscription/AlreadySubscribed

        [Authorize]
        public ActionResult AlreadySubscribed()
        {
            return View();
        }
    }
}
