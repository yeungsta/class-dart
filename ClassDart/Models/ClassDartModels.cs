using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace ClassDart.Models
{
    //need to make serializable since object is used in home index page, and SQL session
    //state stuff serializes this.
    [Serializable]
    public class Class
    {
        public int ID { get; set; }
        [Required(ErrorMessage = "You must enter a class name.")]
        public string Name { get; set; }
        public string Template { get; set; }
        public string Instructor { get; set; }
        public string Url { get; set; }
        [Column(TypeName = "xml")]
        public string Announcements { get; set; }
        [Column(TypeName = "xml")]
        public string Assignments { get; set; }
        [Column(TypeName = "xml")]
        public string Subscribers { get; set; }
    }

    //stored as XML in Class
    public class Announcement
    {
        public int ID { get; set; }
        [Required]
        public string Text { get; set; }
        public DateTime UpdateDateTime { get; set; }
    }

    //stored as XML in Class
    public class Assignment
    {
        public int ID { get; set; }
        [Required]
        public string Title { get; set; }
        public string Text { get; set; }
        public DateTime DueDateTime { get; set; }
        public DateTime UpdateDateTime { get; set; }
    }

    //stored as XML in Class
    public class Subscriber
    {
        public int ID { get; set; }
        [Required]
        public string Email { get; set; }
        //Any future options here
    }

    public class Instructor
    {
        public int ID { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string User { get; set; }
        public string Prefix { get; set; }
        public string FirstName { get; set; }
        public string MiddleInitial { get; set; }
        public string LastName { get; set; }
        public string Handle { get; set; }
        public string Bio { get; set; }
        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [DataType(DataType.Url)]
        public string Facebook { get; set; }
        [DataType(DataType.Url)]
        public string Twitter { get; set; }
        [DataType(DataType.Url)]
        public string LinkedIn { get; set; }
        [DataType(DataType.Url)]
        public string Website { get; set; }
    }

    public class Template
    {
        public int ID { get; set; }
        [Required]
        public string Name { get; set; }
    }

    public class ClassDartDBContext : DbContext
    {
        public DbSet<Class> Classes { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<Template> Templates { get; set; }

        //we won't be storing Announcements as an SQL table, however. Just as XML
        //in the Class table.
        public DbSet<Announcement> Announcements { get; set; }

        //we won't be storing Assignments as an SQL table, however. Just as XML
        //in the Class table.
        public DbSet<Assignment> Assignments { get; set; }

#if Staging  //map to staging database tables
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Class>().ToTable("Classes_staging");
            modelBuilder.Entity<Template>().ToTable("Templates_staging");
            modelBuilder.Entity<Instructor>().ToTable("Instructors_staging");
        }
#endif
    }
}