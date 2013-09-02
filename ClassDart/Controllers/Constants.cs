using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClassDart.Controllers
{
    public class Constants
    {
        public const string AnnouncementsLbl = "Stream";
        public const string AssignmentsLbl = "Assignments";
        public const string InstructorLbl = "Instructor";
        public const string UrlPrefix = @"http://";
        public const string UrlSecurePrefix = @"https://";
        public const string TwitterPrefix = @"https://www.twitter.com/";
        //email
        public const string ReplyEmail = "no-reply@classdart.com";
        public const string SupportEmail = "support@classdart.com";
        //Amazon S3
        public const string AmazonS3BucketName = "my.classdart.com";      
        public const string AmazonS3HtmlObjectType = "text/html";
    }
}