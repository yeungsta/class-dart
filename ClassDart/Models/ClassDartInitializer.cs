using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using ClassDart.Models;
using ClassDart.Composer;

namespace ClassDart.Models
{
    //Use if just want to reinitialize when model changes
    public class ClassDartInitializer : DropCreateDatabaseIfModelChanges<ClassDartDBContext>

    //Use if just want to reinitialize every time
    //public class ClassDartInitializer : DropCreateDatabaseAlways<ClassDartDBContext>
    {
        protected override void Seed(ClassDartDBContext context)
        {
            var templates = new List<Template> {  
                    new Template { Name = "Jalapeno" },
                    new Template { Name = "Eggplant" },
                    new Template { Name = "Ice" },
                    new Template { Name = "Brie" },
                    new Template { Name = "Marinara" },
                    new Template { Name = "Spinach" },
                    new Template { Name = "Licorice" },
                };

            templates.ForEach(s => context.Templates.Add(s));
            context.SaveChanges();

            var instructors = new List<Instructor>
            {
                new Instructor { User = "rebecca.hong@biola.edu", Prefix = "Dr.", FirstName = "Rebecca", MiddleInitial = "C.", LastName = "Hong", Handle = "rebeccahong", Bio = "A graduate of USC, Dr. Hong is interested in the intersection of education and technology.", Phone = "111-123-4567", Email = "rebecca.hong@biola.edu", Facebook = "www.facebook.com", Twitter = "www.twitter.com/FYSProf", LinkedIn = "www.linkedin.com", Website = "www.hotprofessoi.com"  }
            };
            instructors.ForEach(s => context.Instructors.Add(s));
            context.SaveChanges();

            //create announcements
            Announcement announcement1 = new Announcement();
            announcement1.Text = "This is a first announcement.";
            announcement1.UpdateDateTime = DateTime.Parse("2013-04-01");

            List<Announcement> announcements = new List<Announcement>();
            announcements.Add(announcement1);

            //create assignments
            Assignment assignment1 = new Assignment();
            assignment1.Title = "Week 1 Assignment";
            assignment1.Text = "This assignment is very hard.";
            assignment1.DueDateTime = DateTime.Parse("2013-05-01");
            assignment1.UpdateDateTime = DateTime.Parse("2013-04-02");

            List<Assignment> assignments = new List<Assignment>();
            assignments.Add(assignment1);

            var classes = new List<Class>
            {
                new Class { Name = "Research Methods", Url = "rebeccahong/research-methods", Instructor = "rebecca.hong@biola.edu", Template = "Licorice", Announcements = V1.SerializeAnnouncements(announcements), Assignments = V1.SerializeAssignments(assignments) }
            };
            classes.ForEach(s => context.Classes.Add(s));
            context.SaveChanges();
        }
    }
}