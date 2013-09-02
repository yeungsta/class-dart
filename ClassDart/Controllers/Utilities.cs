using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using ClassDart.Models;
using ClassDart.Composer;
using System.Data.Entity;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;

namespace ClassDart.Controllers
{
    public static class Utilities
    {
        //non-blocking emailer
        public static void NotifySubscribers(Class classObj)
        {
#if !NoEmail
            new Thread(() =>
            {
                if (classObj != null)
                {
                    IList<Subscriber> currentSubscribers = V1.DeserializeSubscribers(classObj.Subscribers);

                    if (currentSubscribers != null)
                    {
                        foreach (Subscriber subscriber in currentSubscribers)
                        {
                            //Send email
                            try //TODO: remove try/catch when using real SMTP server in production
                            {
                                //synchronous
                                //new MailController().SendClassUpdateEmail(subscriber.Email, classObj.Name, classObj.ID, classObj.Url).Deliver();

                                //asynchronous (but using DeliverAync doesn't work w/ a real SMTP server for some reason)
                                new MailControllerAsync().SendClassUpdateEmailAsync(subscriber.Email, classObj.Name, classObj.ID, classObj.Url).Deliver();
                            }
                            catch (Exception e)
                            {
                                string hi = e.Message;
                            }
                        } 
                    } 
                }
            }).Start();
#endif
        }

        public static string CreateClassDartUrl(string userHandle, string className)
        {
            //Create unique classdart URL.   
            string tempUrl = userHandle;

            //add directory slash
            tempUrl += "/";

            if (!string.IsNullOrEmpty(className))
            {
                //Replace all spaces with dashes.
                tempUrl += (className.Replace(' ', '-')).ToLower();
            }

            //remove unwanted chars
            tempUrl = tempUrl.Replace(",", "");
            tempUrl = tempUrl.Replace("'", "");
            tempUrl = tempUrl.Replace(".", "");

            //replace chars with text
            tempUrl = tempUrl.Replace("&", "and");

            return tempUrl;
        }

        public static string CreateUniqueInstructorHandle(ClassDartDBContext db, string firstname, string lastname)
        {
            string tempUrl = string.Empty;

            if (!string.IsNullOrEmpty(firstname))
            {
                tempUrl += (firstname.Replace(' ', '-')).ToLower();
            }

            if (!string.IsNullOrEmpty(lastname))
            {
                tempUrl += (lastname.Replace(' ', '-')).ToLower();
            }

            //remove unwanted chars
            tempUrl = tempUrl.Replace(",", "");
            tempUrl = tempUrl.Replace("'", "");
            tempUrl = tempUrl.Replace(".", "");

            //replace chars with text
            tempUrl = tempUrl.Replace("&", "and");

            //Check if there are duplicate handles.
            int matches = db.Instructors.Count(instructorObj => instructorObj.Handle.Contains(tempUrl));

            if (matches > 0)
            {
                tempUrl += (matches + 1).ToString();
            }

            return tempUrl;
        }

        //constructs full URL of class dart page
        public static string GetFullUrl(string classUrl)
        {
#if UseAmazonS3
            return "http://" + Controllers.Constants.AmazonS3BucketName + "/" + classUrl;          
#else
            return "http://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/Content/classes/" + classUrl + "/index.html";
#endif
        }

        public static string Linkify(string SearchText)
        {
            if (!string.IsNullOrEmpty(SearchText))
            {
                // this will find links like:
                // http://www.mysite.com
                // as well as any links with other characters directly in front of it like:
                // href="http://www.mysite.com"
                // you can then use your own logic to determine which links to linkify
                Regex regx = new Regex(@"\b(http://|https://|ftp://|www\.)([^\s()[\]<>]+|\([^\s)]*\)|\[[^\s\]]*])+(?<![.,!?])", RegexOptions.IgnoreCase);
                SearchText = SearchText.Replace("&nbsp;", " ");
                MatchCollection matches = regx.Matches(SearchText);

                IList<string> noDuplicates = new List<string>();

                //remove any dupes
                foreach (Match match in matches)
                {
                    if (!noDuplicates.Contains(match.Value))
                    {
                        noDuplicates.Add(match.Value);
                    }
                }

                foreach (string item in noDuplicates)
                {
                    if (item.StartsWith("http") || item.StartsWith("ftp"))
                    {
                        SearchText = SearchText.Replace(item, "<a href='" + item + "' target='_blank'>" + item + "</a>");
                    }
                    else if (item.StartsWith("www."))
                    {
                        SearchText = SearchText.Replace(item, "<a href='http://" + item + "' target='_blank'>" + item + "</a>");
                    }
                }
            }

            return SearchText;
        }

        public static bool GetClassObjFromId(ClassDartDBContext db, int classId, out Class classObj)
        {
            classObj = db.Classes.Find(classId);

            if (classObj == null)
            {
                return false;
            }

            return true;
        }

        public static IOrderedQueryable<Class> GetInstructorClassesFromEmail(ClassDartDBContext db, string userEmail)
        {
            return from allClasses in db.Classes
                   where allClasses.Instructor == userEmail
                   orderby allClasses.Name ascending
                   select allClasses;
        }

        public static bool GetInstructorObjFromEmail(ClassDartDBContext db, string userEmail, out Instructor instructorObj)
        {
            IOrderedQueryable<Instructor> allInstructors = from allInstructor in db.Instructors
                                                           where allInstructor.User == userEmail
                                                           orderby allInstructor.User ascending
                                                           select allInstructor;

            //there should only be one!
            if (allInstructors == null || allInstructors.Count() != 1)
            {
                instructorObj = null;
                return false;
            }

            instructorObj = allInstructors.Single();
            return true;
        }

        public static void SaveToDb(ClassDartDBContext db, object databaseObj)
        {
            db.Entry(databaseObj).State = EntityState.Modified;
            db.SaveChanges();         
        }

        public static void SaveToClassDart(Class classObj, Instructor instructorObj)
        {
            V1 composer = new V1(classObj, instructorObj);
            // re-compose the classdart page
            composer.CreateClassDart();
            // todo: temp add until Mel's instructor page gets added!
            composer.CreateInstructorPage();
        }

        public static void SaveToInstructorPage(Class classObj, Instructor instructorObj)
        {
            V1 composer = new V1(classObj, instructorObj);
            // re-compose the instructor page
            composer.CreateInstructorPage();
        }

        public static void SaveToClassDartAndInstructorPage(Class classObj, Instructor instructorObj)
        {
            V1 composer = new V1(classObj, instructorObj);
            // re-compose the classdart page
            composer.CreateClassDart();
            composer.CreateInstructorPage();
        }

        //Cleans web URLs (remove "http://" or "https://")
        public static string CleanUrl(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                string trimmed = url.Trim();

                if (trimmed.StartsWith(Constants.UrlPrefix))
                {
                    return trimmed.Replace(Constants.UrlPrefix, "");
                }
                else if (trimmed.StartsWith(Constants.UrlSecurePrefix))
                {
                    return trimmed.Replace(Constants.UrlSecurePrefix, "");
                }

                return trimmed;
            }

            return url;
        }

        //Cleans Twitter usernames (extract username if entire URL)
        public static string CleanTwitter(string username)
        {
            if (!string.IsNullOrEmpty(username))
            {
                string trimmed = username.Trim();

                if (trimmed.StartsWith(Constants.UrlPrefix))
                {
                    string[] urlParts = trimmed.Split('/');
                    return urlParts[urlParts.Count() - 1];
                }
                else if (trimmed.StartsWith(Constants.UrlSecurePrefix))
                {
                    string[] urlParts = trimmed.Split('/');
                    return urlParts[urlParts.Count() - 1];
                }

                return trimmed;
            }

            return username;
        }

        //Adds "http://"
        public static string AddHttpPrefix(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                if (!url.StartsWith(Constants.UrlPrefix))
                {
                    return (Constants.UrlPrefix + url);
                }

                return url;
            }

            return url;
        }

        //Adds "http://www.twitter.com/<username>"
        public static string AddTwitterPrefix(string username)
        {
            if (!string.IsNullOrEmpty(username))
            {
                if (!username.StartsWith(Constants.UrlPrefix))
                {
                    return (Constants.TwitterPrefix + username);
                }
                else if (!username.StartsWith(Constants.UrlSecurePrefix))
                {
                    return (Constants.TwitterPrefix + username);
                }

                return username;
            }

            return username;
        }

        public static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "Email address already exists. Please enter a different email address.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    //Utilities.LogAppError("An authentication provider error occurred while trying to create a new user.");
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    //Utilities.LogAppError("An user rejected error occurred while trying to create a new user.");
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    //Utilities.LogAppError("An unknown error occurred while trying to create a new user.");
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }

        /* The following methods are for Amazon S3 filesystem */

        //writes plaintext data to S3 cloud
        public static void WritePlainTextObjectToS3(string data, string filePath)
        {
            AmazonS3 client;

            if (CheckS3Credentials())
            {
                NameValueCollection appConfig =
                    ConfigurationManager.AppSettings;

                string accessKeyID = appConfig["AWSAccessKey"];
                string secretAccessKeyID = appConfig["AWSSecretKey"];

                using (client = Amazon.AWSClientFactory.CreateAmazonS3Client(
                    accessKeyID, secretAccessKeyID, RegionEndpoint.USWest1))
                {
                    try
                    {
                        PutObjectRequest request = new PutObjectRequest();
                        request.WithContentBody(data)
                            .WithBucketName(Constants.AmazonS3BucketName)
                            .WithKey(filePath)
                            .WithContentType(Controllers.Constants.AmazonS3HtmlObjectType)
                            .WithCannedACL(S3CannedACL.PublicRead);

                        S3Response response3 = client.PutObject(request);
                        response3.Dispose();
                    }
                    catch (AmazonS3Exception amazonS3Exception)
                    {
                        if (amazonS3Exception.ErrorCode != null &&
                            (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                            ||
                            amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                        {
                            Console.WriteLine("Check the provided AWS Credentials.");
                            Console.WriteLine(
                                "For service sign up go to http://aws.amazon.com/s3");
                        }
                        else
                        {
                            Console.WriteLine(
                                "Error occurred. Message:'{0}' when writing an object"
                                , amazonS3Exception.Message);
                        }
                    }
                }
            }
        }

        //checks if file exists S3 cloud
        public static bool IsObjectExistS3(string filePath)
        {
            AmazonS3 client;

            if (CheckS3Credentials())
            {
                NameValueCollection appConfig =
                    ConfigurationManager.AppSettings;

                string accessKeyID = appConfig["AWSAccessKey"];
                string secretAccessKeyID = appConfig["AWSSecretKey"];

                using (client = Amazon.AWSClientFactory.CreateAmazonS3Client(
                    accessKeyID, secretAccessKeyID, RegionEndpoint.USWest1))
                {
                    try
                    {
                        S3Response response = client.GetObjectMetadata(new GetObjectMetadataRequest()
                           .WithBucketName(Constants.AmazonS3BucketName)
                           .WithKey(filePath));

                        return true;
                    }
                    catch (Amazon.S3.AmazonS3Exception ex)
                    {
                        if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)

                            return false;
                    }
                }
            }

            return false;
        }

        //removes a file in S3 cloud
        public static void RemoveObjectFromS3(string filePath)
        {
            AmazonS3 client;

            if (CheckS3Credentials())
            {
                NameValueCollection appConfig =
                    ConfigurationManager.AppSettings;

                string accessKeyID = appConfig["AWSAccessKey"];
                string secretAccessKeyID = appConfig["AWSSecretKey"];

                using (client = Amazon.AWSClientFactory.CreateAmazonS3Client(
                    accessKeyID, secretAccessKeyID, RegionEndpoint.USWest1))
                {
                    try
                    {
                        DeleteObjectRequest request = new DeleteObjectRequest();
                        request.WithBucketName(Constants.AmazonS3BucketName)
                            .WithKey(filePath);

                        S3Response response = client.DeleteObject(request);
                        response.Dispose();
                    }
                    catch (AmazonS3Exception amazonS3Exception)
                    {
                        if (amazonS3Exception.ErrorCode != null &&
                            (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                            ||
                            amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                        {
                            Console.WriteLine("Check the provided AWS Credentials.");
                            Console.WriteLine(
                                "For service sign up go to http://aws.amazon.com/s3");
                        }
                        else
                        {
                            Console.WriteLine(
                                "Error occurred. Message:'{0}' when deleting an object"
                                , amazonS3Exception.Message);
                        }
                    }
                }
            }
        }

        static bool CheckS3Credentials()
        {
            NameValueCollection appConfig = ConfigurationManager.AppSettings;

            if (string.IsNullOrEmpty(appConfig["AWSAccessKey"]))
            {
                Console.WriteLine(
                    "AWSAccessKey was not set in the App.config file.");
                return false;
            }
            if (string.IsNullOrEmpty(appConfig["AWSSecretKey"]))
            {
                Console.WriteLine(
                    "AWSSecretKey was not set in the App.config file.");
                return false;
            }
            if (string.IsNullOrEmpty(Constants.AmazonS3BucketName))
            {
                Console.WriteLine("The variable bucketName is not set.");
                return false;
            }

            return true;
        }
    }
}