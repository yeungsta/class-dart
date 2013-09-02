using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClassDart.Models
{
    public class EditorViewModel
    {
        //data
        public Class classObj { get; set; }
        public Instructor instructorObj { get; set; }
        public IList<Announcement> announcements { get; set; }
        public IList<Assignment> assignments { get; set; }
        public IList<Subscriber> subscribers { get; set; }
        public string fullUrl { get; set; }

        //labels
        public string AnnouncementsLbl { get; set; }
        public string AssignmentsLbl { get; set; }
        public string InstructorLbl { get; set; }

        //announcement create form
        public string AnnouncementField { get; set; }

        //assignment create form
        public string AssignmentTitleField { get; set; }
        public string AssignmentTextField { get; set; }

        //instructor editor forms
        public string InstructorBioField { get; set; }

        public bool RememberMe { get; set; }
    }
}