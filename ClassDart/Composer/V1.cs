using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.UI;
using System.Xml;
using System.Xml.Serialization;
using ClassDart.Models;
using ClassDart.Controllers;

namespace ClassDart.Composer
{
    public class V1
    {
        private List<Announcement> m_announcements;
        private List<Assignment> m_assignments;
        private List<Subscriber> m_subscribers;
        private static XmlSerializer m_announcementsSerializer;
        private static XmlSerializer m_assignmentsSerializer;
        private static XmlSerializer m_subscribersSerializer;
        private Class m_class;
        private Instructor m_instructor;
        private ClassDartDBContext m_db;

        private enum MenuBarItems
        {
            Stream,
            Assignments,
            Instructor
        }

        #region static members

        /// <summary>
        /// Static Constructor
        /// </summary>
        static V1()
        {
            m_announcementsSerializer = new XmlSerializer(typeof(List<Announcement>));
            m_assignmentsSerializer = new XmlSerializer(typeof(List<Assignment>));
            m_subscribersSerializer = new XmlSerializer(typeof(List<Subscriber>));
        }

        //local
        //public static string SubscriptionUrl = "http://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/Subscribe/Index";
        public static string SubscriptionUrl = "http://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/Subscribe/Index";
        public static string UnsubscribeUrl = "http://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/Subscribe/Unsubscribe";

        public static string SerializeAnnouncements(IList<Announcement> announcements)
        {
            //serialize
            string xmlString;

            using (StringWriter textWriter = new StringWriter())
            {
                m_announcementsSerializer.Serialize(textWriter, announcements);
                xmlString = textWriter.ToString();
            }

            return xmlString;
        }

        public static List<Announcement> DeserializeAnnouncements(string announcements)
        {
            if (!string.IsNullOrEmpty(announcements))
            {
                //deserialize XML announcements data into list of Announcement objects
                if (!string.IsNullOrEmpty(announcements))
                {
                    using (StringReader textReader = new StringReader(announcements))
                    {
                        object output = m_announcementsSerializer.Deserialize(textReader);
                        return output as List<Announcement>;
                    }
                }
            }

            return new List<Announcement>();
        }

        public static string SerializeAssignments(IList<Assignment> assignments)
        {
            //serialize
            string xmlString;

            using (StringWriter textWriter = new StringWriter())
            {
                m_assignmentsSerializer.Serialize(textWriter, assignments);
                xmlString = textWriter.ToString();
            }

            return xmlString;
        }

        public static List<Assignment> DeserializeAssignments(string assignments)
        {
            if (!string.IsNullOrEmpty(assignments))
            {
                //deserialize XML assignments data into list of Assignment objects
                using (StringReader textReader = new StringReader(assignments))
                {
                    object output = m_assignmentsSerializer.Deserialize(textReader);
                    return output as List<Assignment>;
                }
            }

            return new List<Assignment>();
        }

        public static string SerializeSubscribers(IList<Subscriber> subscribers)
        {
            //serialize
            string xmlString;

            using (StringWriter textWriter = new StringWriter())
            {
                m_subscribersSerializer.Serialize(textWriter, subscribers);
                xmlString = textWriter.ToString();
            }

            return xmlString;
        }

        public static List<Subscriber> DeserializeSubscribers(string subscribers)
        {
            if (!string.IsNullOrEmpty(subscribers))
            {
                //deserialize XML subscribers data into list of Subscriber objects
                if (!string.IsNullOrEmpty(subscribers))
                {
                    using (StringReader textReader = new StringReader(subscribers))
                    {
                        object output = m_subscribersSerializer.Deserialize(textReader);
                        return output as List<Subscriber>;
                    }
                }
            }

            return new List<Subscriber>();
        }

        #endregion static members

        #region public members

        /// <summary>
        /// Constructor
        /// </summary>
        public V1(Class classObj, Instructor instructorObj)
        {
            if (classObj != null)
            {
                m_db = new ClassDartDBContext();
                m_class = classObj;

                if (instructorObj != null)
                {
                    m_instructor = instructorObj;
                }
                else
                {
                    //instructor not provided, so get it
                    Utilities.GetInstructorObjFromEmail(m_db, classObj.Instructor, out instructorObj);
                    m_instructor = instructorObj;
                }
            }
        }

        public string CreateClassDart()
        {
#if UseAmazonS3
            string urlpath = Utilities.GetFullUrl(m_class.Url);

            //write class page string to S3
            Utilities.WritePlainTextObjectToS3(RenderClassDart(), m_class.Url + "/index.html");
#else
            string urlpath = Utilities.GetFullUrl(m_class.Url);

            //active directories
            string filepath = HttpContext.Current.Server.MapPath("~/Content/classes/" + m_class.Url + "/");

            //write HTML string to file
            WriteToFile(RenderClassDart(), filepath);
#endif
            // return URL to classdart
            return urlpath;
        }

        public string CreateInstructorPage()
        {
#if UseAmazonS3
            string urlpath = Utilities.GetFullUrl(m_instructor.Handle);

            //write instructor page string to S3
            Utilities.WritePlainTextObjectToS3(RenderInstructorPage(), m_instructor.Handle + "/index.html");
#else
            string urlpath = Utilities.GetFullUrl(m_instructor.Handle);

            //active directories
            string filepath = HttpContext.Current.Server.MapPath("~/Content/classes/" + m_instructor.Handle + "/");

            //write HTML string to file
            WriteToFile(RenderInstructorPage(), filepath);
#endif
            // return URL to classdart
            return urlpath;
        }

        #endregion public members

        #region private members

        //actual HTML creation/rendering
        private string RenderClassDart()
        {
            StringWriter stringWriter = new StringWriter();

            //just have one space as the tab
            using (HtmlTextWriter writer = new HtmlTextWriter(stringWriter, " "))
            {
                Initialize();
                CreateBase(writer, m_class.Name);
                CreateStream(writer);
                writer.WriteLine();
                writer.WriteLine();
                CreateAssignments(writer);
                writer.WriteLine();
                writer.WriteLine();
                CreateInstructor(writer);
                writer.WriteLine();
                writer.WriteLine();
                AddAnnouncementPages(writer, m_announcements);
                writer.WriteLine();
                AddSubscriptionForm(writer);
                writer.RenderEndTag(); // </body>
                writer.RenderEndTag(); // </html>
            }

            return stringWriter.ToString();
        }

        //actual HTML creation/rendering
        private string RenderInstructorPage()
        {
            StringWriter stringWriter = new StringWriter();

            //just have one space as the tab
            using (HtmlTextWriter writer = new HtmlTextWriter(stringWriter, " "))
            {
                Initialize();
                string instructorFullDisplayName = 
                    m_instructor.Prefix + " " + m_instructor.FirstName + " " +
                    m_instructor.MiddleInitial + " " + m_instructor.LastName;
                CreateBase(writer, instructorFullDisplayName);
                BeginPage(writer, null, "bodyBg", string.Empty);
                BeginHeader(writer, string.Empty, string.Empty);
                AddHeaderBar(writer, "headerBar", instructorFullDisplayName);
                writer.RenderEndTag();
                writer.WriteLine();
                BeginContent(writer, null);
                writer.RenderBeginTag(HtmlTextWriterTag.Center);
                writer.Write(Utilities.Linkify(m_instructor.Bio));
                writer.RenderEndTag();
                writer.WriteBreak();
                AddInstructorInfoButtons(writer, m_instructor);
                writer.WriteBreak();
                AddClassList(writer);
                writer.RenderEndTag();
                writer.WriteBreak();
                writer.WriteBreak();
                writer.WriteBreak();
                writer.WriteLine();
                AddInstructorPageFooter(writer);
                writer.RenderEndTag(); // </body>
                writer.RenderEndTag(); // </html>
            }

            return stringWriter.ToString();
        }

        private void Initialize()
        {
            //deserialize XML announcements data into list
            if (!string.IsNullOrEmpty(m_class.Announcements))
            {
                m_announcements = DeserializeAnnouncements(m_class.Announcements);
            }

            //deserialize XML assignments data into list
            if (!string.IsNullOrEmpty(m_class.Assignments))
            {
                m_assignments = DeserializeAssignments(m_class.Assignments);
            }
        }

        private void CreateBase(HtmlTextWriter writer, string title)
        {
            writer.Write(Constants.DocType);
            writer.WriteLine();
            writer.RenderBeginTag(HtmlTextWriterTag.Html);
            writer.RenderBeginTag(HtmlTextWriterTag.Head);
            AddTitle(writer, title);
            AddMetaEncoding(writer);
            AddMeta(writer, "viewport", "width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=0");
            AddMeta(writer, "apple-mobile-web-app-capable", "yes");
            AddMeta(writer, "apple-mobile-web-app-status-bar-style", "black");
            AddLink(writer, "stylesheet", Constants.JqueryMobileCss, null);
            AddLink(writer, "stylesheet", Constants.MasterCssPath, "text/css");

            //if a template is defined, add template stylesheet
            if (!string.IsNullOrEmpty(m_class.Template))
            {
                AddLink(writer, "stylesheet", Constants.TemplateCssPath + Constants.TemplateCssName + m_class.Template.ToLower() + Constants.CssExtension, "text/css");
            }
            else
            {
                AddLink(writer, "stylesheet", Constants.TemplateCssPath + Constants.TemplateCssName + Constants.DefaultTemplate + Constants.CssExtension, "text/css");
            }

            AddScript(writer, Constants.Jquery);
            AddScript(writer, Constants.JqueryMobile);
            writer.RenderEndTag();
            writer.WriteLine();
            writer.RenderBeginTag(HtmlTextWriterTag.Body);
        }

        private void CreateStream(HtmlTextWriter writer)
        {
            BeginPage(writer, MenuBarItems.Stream.ToString(), "bodyBg", string.Empty);
            BeginHeader(writer, string.Empty, string.Empty);
            AddHeaderBar(writer, "headerBar", m_class.Name);
            writer.WriteLine();
            AddMenuBar(writer, MenuBarItems.Stream);
            writer.RenderEndTag();
            writer.WriteLine();
            BeginContent(writer, null);
            AddAnnouncementsList(writer, m_announcements);
            writer.RenderEndTag();
            writer.WriteBreak();
            writer.WriteBreak();
            writer.WriteBreak();
            writer.WriteLine();
            AddFooter(writer);
            writer.RenderEndTag();
        }

        private void CreateAssignments(HtmlTextWriter writer)
        {
            BeginPage(writer, MenuBarItems.Assignments.ToString(), "bodyBg", string.Empty);
            BeginHeader(writer, null, null);
            AddHeaderBar(writer, "headerBar", m_class.Name);
            writer.WriteLine();
            AddMenuBar(writer, MenuBarItems.Assignments);
            writer.RenderEndTag();
            writer.WriteLine();
            BeginContent(writer, null);
            AddAssignmentsList(writer, m_assignments); 
            writer.RenderEndTag();
            writer.WriteBreak();
            writer.WriteBreak();
            writer.WriteBreak();
            writer.WriteLine();
            AddFooter(writer);
            writer.RenderEndTag();
        }

        private void CreateInstructor(HtmlTextWriter writer)
        {
            BeginPage(writer, MenuBarItems.Instructor.ToString(), "bodyBg", string.Empty);
            BeginHeader(writer, null, null);
            AddHeaderBar(writer, "headerBar", m_class.Name);
            writer.WriteLine();
            AddMenuBar(writer, MenuBarItems.Instructor);
            writer.RenderEndTag(); //end header div
            writer.WriteLine();
            BeginContent(writer, null);
            AddInstructorInfo(writer, m_instructor);
            AddInstructorInfoButtons(writer, m_instructor);
            writer.RenderEndTag(); //end content div
            writer.WriteBreak();
            writer.WriteBreak();
            writer.WriteBreak();
            writer.WriteLine();
            AddFooter(writer);
            writer.RenderEndTag(); //end page div
        }

        private void BeginPage(HtmlTextWriter writer, string id, string className, string theme)
        {
            writer.AddAttribute(Constants.DataRole, Constants.PageRole);
            if (!string.IsNullOrEmpty(id)) { writer.AddAttribute(HtmlTextWriterAttribute.Id, id); }
            if (!string.IsNullOrEmpty(className)) { writer.AddAttribute(HtmlTextWriterAttribute.Class, className); }
            if (!string.IsNullOrEmpty(theme)) { writer.AddAttribute(Constants.DataTheme, theme); }
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
        }

        private void BeginHeader(HtmlTextWriter writer, string className, string theme)
        {
            writer.AddAttribute(Constants.DataRole, Constants.HeaderRole);
            if (!string.IsNullOrEmpty(className)) { writer.AddAttribute(HtmlTextWriterAttribute.Class, className); }
            if (!string.IsNullOrEmpty(theme)) { writer.AddAttribute(Constants.DataTheme, theme); }
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
        }

        private void BeginContent(HtmlTextWriter writer, string theme)
        {
            writer.AddAttribute(Constants.DataRole, Constants.ContentRole);
            if (!string.IsNullOrEmpty(theme)) { writer.AddAttribute(Constants.DataTheme, theme); }
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
        }

        private void BeginForm(HtmlTextWriter writer, string action)
        {
            writer.AddAttribute(Constants.FormAction, action);
            writer.AddAttribute(Constants.FormMethod, Constants.Post);
            writer.RenderBeginTag(HtmlTextWriterTag.Form);
        }

        private void BeginFieldset(HtmlTextWriter writer)
        {
            writer.RenderBeginTag(HtmlTextWriterTag.Fieldset);
        }

        private void AddAnnouncementsList(HtmlTextWriter writer, List<Announcement> announcements)
        {
            writer.AddAttribute(Constants.DataRole, "listview");
            writer.AddAttribute(Constants.DataInset, "true");
            writer.RenderBeginTag(HtmlTextWriterTag.Ul);

            if (announcements != null)
            {
                DateTime lastDate = DateTime.MinValue;

                for (int i = announcements.Count() - 1; i >= 0 ; i--)
                {
                    if (lastDate != announcements[i].UpdateDateTime.Date)
                    {
                        lastDate = announcements[i].UpdateDateTime.Date;
                        AddAnnouncementDivider(writer, announcements[i].UpdateDateTime);
                    }

                    writer.RenderBeginTag(HtmlTextWriterTag.Li);
                    writer.WriteLine();
                    writer.Indent++;
                    AddAnnouncement(writer, announcements[i].Text, announcements[i].UpdateDateTime, "#announcement" + i);
                    writer.Indent--;
                    writer.RenderEndTag();
                    writer.WriteLine();
                }
            }

            writer.RenderEndTag();
        }

        private void AddClass(HtmlTextWriter writer, string text, string href)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Href, href);
            writer.AddAttribute(Constants.DataTransition, Constants.Slide);
            writer.AddAttribute(HtmlTextWriterAttribute.Target, Constants.BlankTarget);
            writer.RenderBeginTag(HtmlTextWriterTag.A);
            writer.RenderBeginTag(HtmlTextWriterTag.H2);
            writer.Write(text);
            writer.RenderEndTag();
            writer.RenderEndTag();
        }

        private void AddAnnouncement(HtmlTextWriter writer, string text, DateTime updateDateTime, string href)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Href, href);
            writer.AddAttribute(Constants.DataTransition, Constants.Slide);
            writer.RenderBeginTag(HtmlTextWriterTag.A);
            writer.RenderBeginTag(HtmlTextWriterTag.H2);
            writer.Write(text);
            writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.P);
            writer.RenderBeginTag(HtmlTextWriterTag.Strong);
            writer.Write(updateDateTime.ToString("h:mm tt"));
            writer.RenderEndTag();
            writer.RenderEndTag();
            writer.RenderEndTag();
        }

        private void AddAnnouncementDivider(HtmlTextWriter writer, DateTime updateDateTime)
        {
            writer.AddAttribute(Constants.DataRole, Constants.ListDivider);
            writer.RenderBeginTag(HtmlTextWriterTag.Li);
            writer.Write(updateDateTime.ToString("dddd, MMMM dd, yyyy"));
            writer.RenderEndTag();
        }

        private void AddAnnouncementPages(HtmlTextWriter writer, List<Announcement> announcements)
        {
            if (announcements != null)
            {
                for (int i = 0; i < announcements.Count(); i++)
                {
                    AddAnnouncementPage(writer, announcements[i], "announcement" + i);
                    writer.WriteLine();
                    writer.WriteLine();
                }
            }
        }

        private void AddAnnouncementPage(HtmlTextWriter writer, Announcement announcement, string id)
        {
            BeginPage(writer, id, "bodyBg", string.Empty);
            BeginHeader(writer, null, null);
            AddHeaderBar(writer, "headerBar", m_class.Name);
            writer.WriteLine();
            AddAnnouncementPageMenuBar(writer);
            writer.RenderEndTag();
            writer.WriteLine();
            BeginContent(writer, null);
            AddAnnouncementPageItem(writer, announcement);
            writer.RenderEndTag();
            writer.RenderEndTag();
        }

        private void AddAnnouncementPageItem(HtmlTextWriter writer, Announcement announcement)
        {
            writer.AddAttribute(Constants.DataRole, "listview");
            writer.AddAttribute(Constants.DataInset, "true");
            writer.RenderBeginTag(HtmlTextWriterTag.Ul);
            writer.RenderBeginTag(HtmlTextWriterTag.Li);
            writer.WriteLine();
            writer.Indent++;
            writer.RenderBeginTag(HtmlTextWriterTag.H2);
            writer.AddAttribute(HtmlTextWriterAttribute.Class, Constants.ShowAllTextClass);
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            writer.Write(Utilities.Linkify(announcement.Text));
            writer.RenderEndTag();
            writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.P);
            writer.RenderBeginTag(HtmlTextWriterTag.Strong);
            writer.Write(announcement.UpdateDateTime.ToString("h:mm tt, dddd, MMMM dd, yyyy"));
            writer.RenderEndTag();
            writer.RenderEndTag();
            writer.Indent--;
            writer.RenderEndTag();
            writer.WriteLine();
            writer.RenderEndTag();
        }

        private void AddAssignmentsList(HtmlTextWriter writer, List<Assignment> assignments)
        {
            if (assignments != null)
            {
                for (int i = 0; i < assignments.Count(); i++)
                {
                    writer.AddAttribute(Constants.DataRole, Constants.CollapsibleRole);
                    writer.AddAttribute(Constants.DataTheme, "b");
                    writer.AddAttribute(Constants.DataContentTheme, "c");
                    writer.RenderBeginTag(HtmlTextWriterTag.Div);
                    writer.RenderBeginTag(HtmlTextWriterTag.H2);
                    writer.Write(assignments[i].Title);
                    writer.RenderEndTag();
                    writer.RenderBeginTag(HtmlTextWriterTag.P);
                    writer.Write(Utilities.Linkify(assignments[i].Text));
                    writer.RenderEndTag();                    
                    writer.RenderEndTag();
                    writer.WriteLine();
                    writer.WriteBreak();
                }
            }
        }

        private void AddClassList(HtmlTextWriter writer)
        {
            writer.AddAttribute(Constants.DataRole, "listview");
            writer.AddAttribute(Constants.DataInset, "true");
            writer.RenderBeginTag(HtmlTextWriterTag.Ul);
            //add title bar
            writer.AddAttribute(Constants.DataRole, Constants.ListDivider);
            writer.RenderBeginTag(HtmlTextWriterTag.Li);
            writer.Write("Instructor Classes");
            writer.RenderEndTag();

            IOrderedQueryable<Class> classes = Utilities.GetInstructorClassesFromEmail(m_db, m_instructor.User);

            if (classes.Count() == 0)
            {
                    writer.RenderBeginTag(HtmlTextWriterTag.Li);
                    writer.WriteLine();
                    writer.Indent++;
                    writer.Write("Instructor does not have any classes yet.");
                    writer.Indent--;
                    writer.RenderEndTag();
                    writer.WriteLine();
            }
            else
            {
                foreach (Class item in classes)
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Li);
                    writer.WriteLine();
                    writer.Indent++;
                    AddClass(writer, item.Name, Utilities.GetFullUrl(item.Url) + "/");
                    writer.Indent--;
                    writer.RenderEndTag();
                    writer.WriteLine();
                }
            }

            writer.RenderEndTag();
        }

        private void AddInstructorInfo(HtmlTextWriter writer, Instructor instructorObj)
        {
            writer.RenderBeginTag(HtmlTextWriterTag.Center);
            writer.RenderBeginTag(HtmlTextWriterTag.H3);
            writer.Write(instructorObj.Prefix);
            writer.Write(" ");
            writer.Write(instructorObj.FirstName);
            writer.Write(" ");
            writer.Write(instructorObj.MiddleInitial);
            writer.Write(" ");
            writer.Write(instructorObj.LastName);
            writer.RenderEndTag();
            writer.WriteBreak();
            writer.Write(Utilities.Linkify(instructorObj.Bio));
            writer.RenderEndTag();
            writer.WriteBreak();
        }

        private void AddInstructorInfoButtons(HtmlTextWriter writer, Instructor instructorObj)
        {
            writer.RenderBeginTag(HtmlTextWriterTag.Center);
            if (!string.IsNullOrEmpty(instructorObj.Email))
            {
                AddBtn(writer, "Email", "mailto:" + instructorObj.Email, Constants.EmailIcon, "b", string.Empty);
            }
            if (!string.IsNullOrEmpty(instructorObj.Phone))
            {
                AddBtn(writer, "Phone", "tel:" + instructorObj.Phone, Constants.CallIcon, "b", string.Empty);
            }
            if (!string.IsNullOrEmpty(instructorObj.Facebook))
            {
                AddBtn(writer, "Facebook", Utilities.AddHttpPrefix(instructorObj.Facebook), Constants.FacebookIcon, "b", Constants.BlankTarget);
            }
            if (!string.IsNullOrEmpty(instructorObj.LinkedIn))
            {
                AddBtn(writer, "LinkedIn", Utilities.AddHttpPrefix(instructorObj.LinkedIn), Constants.LinkedInIcon, "b", Constants.BlankTarget);
            }
            if (!string.IsNullOrEmpty(instructorObj.Twitter))
            {
                AddBtn(writer, "Twitter", Utilities.AddTwitterPrefix(instructorObj.Twitter), Constants.TwitterIcon, "b", Constants.BlankTarget);
            }
            if (!string.IsNullOrEmpty(instructorObj.Website))
            {
                AddBtn(writer, "Website", Utilities.AddHttpPrefix(instructorObj.Website), Constants.OrigSiteIcon, "b", Constants.BlankTarget);
            }
            writer.RenderEndTag();
        }

        private void AddBtn(HtmlTextWriter writer, string label, string href, string icon, string theme, string target)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Href, href);
            writer.AddAttribute(Constants.DataRole, Constants.ButtonRole);
            writer.AddAttribute(Constants.DataIcon, icon);
            if (!string.IsNullOrEmpty(theme)) { writer.AddAttribute(Constants.DataTheme, theme); }
            if (!string.IsNullOrEmpty(target)) { writer.AddAttribute(HtmlTextWriterAttribute.Target, target); }
            writer.AddAttribute(Constants.DataInline, "true");
            writer.RenderBeginTag(HtmlTextWriterTag.A);
            writer.Write(label);
            writer.RenderEndTag();
        }

        private void AddBtnBack(HtmlTextWriter writer, string label, string href, string icon, string theme, string target)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Href, href);
            writer.AddAttribute(Constants.DataRole, Constants.ButtonRole);
            writer.AddAttribute(Constants.DataIcon, icon);
            if (!string.IsNullOrEmpty(theme)) { writer.AddAttribute(Constants.DataTheme, theme); }
            if (!string.IsNullOrEmpty(target)) { writer.AddAttribute(HtmlTextWriterAttribute.Target, target); }
            //writer.AddAttribute(Constants.DataInline, "true");
            writer.AddAttribute(Constants.DataRel, "back");
            writer.RenderBeginTag(HtmlTextWriterTag.A);
            writer.Write(label);
            writer.RenderEndTag();
        }

        private void AddBtnDialog(HtmlTextWriter writer, string label, string href, string transition, string theme)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Href, href);
            writer.AddAttribute(Constants.DataRole, Constants.ButtonRole);
            writer.AddAttribute(Constants.DataTransition, transition);
            writer.AddAttribute(Constants.DataRel, "dialog");
            if (!string.IsNullOrEmpty(theme)) { writer.AddAttribute(Constants.DataTheme, theme); }
            writer.RenderBeginTag(HtmlTextWriterTag.A);
            writer.Write(label);
            writer.RenderEndTag();
        }

        private void AddMiniBtn(HtmlTextWriter writer, string label, string href, string icon, string theme, string target)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Href, href);
            writer.AddAttribute(Constants.DataRole, Constants.ButtonRole);
            writer.AddAttribute(Constants.DataIcon, icon);
            writer.AddAttribute(Constants.DataMini, "true");
            if (!string.IsNullOrEmpty(theme)) { writer.AddAttribute(Constants.DataTheme, theme); }
            if (!string.IsNullOrEmpty(target)) { writer.AddAttribute(HtmlTextWriterAttribute.Target, target); }
            writer.AddAttribute(Constants.DataInline, "true");
            writer.RenderBeginTag(HtmlTextWriterTag.A);
            writer.Write(label);
            writer.RenderEndTag();
        }

        private void AddHeaderBar(HtmlTextWriter writer, string className, string title)
        {
            writer.AddAttribute(Constants.DataRole, Constants.NavbarRole);
            if (!string.IsNullOrEmpty(className)) { writer.AddAttribute(HtmlTextWriterAttribute.Class, className); }
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            AddHeaderTitle(writer, title);
            writer.RenderEndTag();
        }

        private void AddHeaderTitle(HtmlTextWriter writer, string title)
        {
            writer.RenderBeginTag(HtmlTextWriterTag.Center);
            writer.RenderBeginTag(HtmlTextWriterTag.H2);
            writer.Write(title);
            writer.RenderEndTag();
            writer.RenderEndTag();
        }

        private void AddMenuBar(HtmlTextWriter writer, MenuBarItems menuBarItemSelected)
        {
            writer.AddAttribute(Constants.DataRole, Constants.NavbarRole);
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "ui-state-persist");
            writer.AddAttribute(Constants.DataId, "MenuBar");
            writer.AddAttribute(Constants.DataPosition, Constants.PositionFixed);
            writer.AddAttribute(Constants.DataIconPosition, Constants.PositionBottom);     
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.RenderBeginTag(HtmlTextWriterTag.Ul);
            writer.RenderBeginTag(HtmlTextWriterTag.Li);
            AddMenuBarItem(writer, MenuBarItems.Stream.ToString(),
                "#" + MenuBarItems.Stream.ToString(), Constants.StreamIcon, Constants.SlideDown,
                IsMenuBarItemSelected(MenuBarItems.Stream.ToString(), menuBarItemSelected));
            writer.RenderEndTag();
            writer.WriteLine();
            writer.RenderBeginTag(HtmlTextWriterTag.Li);
            AddMenuBarItem(writer, MenuBarItems.Assignments.ToString(),
                "#" + MenuBarItems.Assignments.ToString(), Constants.AssignmentsIcon, Constants.SlideDown,
                IsMenuBarItemSelected(MenuBarItems.Assignments.ToString(), menuBarItemSelected));
            writer.RenderEndTag();
            writer.WriteLine();
            writer.RenderBeginTag(HtmlTextWriterTag.Li);
            AddMenuBarItem(writer, MenuBarItems.Instructor.ToString(),
                "#" + MenuBarItems.Instructor.ToString(), Constants.InstructorIcon, Constants.SlideDown,
                IsMenuBarItemSelected(MenuBarItems.Instructor.ToString(), menuBarItemSelected));
            writer.RenderEndTag();
            writer.RenderEndTag();
            writer.RenderEndTag();
        }

        private void AddAnnouncementPageMenuBar(HtmlTextWriter writer)
        {
            writer.AddAttribute(Constants.DataRole, Constants.NavbarRole);
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "ui-state-persist");
            writer.AddAttribute(Constants.DataId, "MenuBar");
            writer.AddAttribute(Constants.DataPosition, Constants.PositionFixed);
            writer.AddAttribute(Constants.DataIconPosition, Constants.PositionBottom);
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.RenderBeginTag(HtmlTextWriterTag.Ul);
            writer.RenderBeginTag(HtmlTextWriterTag.Li);
            AddMenuBarBackItem(writer, "Back",
                "#" + MenuBarItems.Stream.ToString(), Constants.BackIcon, Constants.GoBack);
            writer.RenderEndTag();
            writer.WriteLine();
            writer.RenderBeginTag(HtmlTextWriterTag.Li);
            AddMenuBarItem(writer, MenuBarItems.Assignments.ToString(),
                "#" + MenuBarItems.Assignments.ToString(), Constants.AssignmentsIcon, Constants.SlideDown, false);
            writer.RenderEndTag();
            writer.WriteLine();
            writer.RenderBeginTag(HtmlTextWriterTag.Li);
            AddMenuBarItem(writer, MenuBarItems.Instructor.ToString(),
                "#" + MenuBarItems.Instructor.ToString(), Constants.InstructorIcon, Constants.SlideDown, false);
            writer.RenderEndTag();
            writer.RenderEndTag();
            writer.RenderEndTag();
        }

        private bool IsMenuBarItemSelected(string currentBtn, MenuBarItems menuBarItemSelected)
        {
            return (currentBtn == menuBarItemSelected.ToString());
        }

        private void AddMenuBarItem(HtmlTextWriter writer, string label, string href, string icon, string transition, bool persist)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Href, href);
            writer.AddAttribute(Constants.DataIcon, icon);
            writer.AddAttribute(Constants.DataTransition, transition);
            if (persist) { writer.AddAttribute(HtmlTextWriterAttribute.Class, "ui-state-persist ui-btn-active"); }
            writer.RenderBeginTag(HtmlTextWriterTag.A);
            writer.Write(label);
            writer.RenderEndTag();
        }

        private void AddMenuBarBackItem(HtmlTextWriter writer, string label, string href, string icon, string rel)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Href, href);
            writer.AddAttribute(Constants.DataIcon, icon);
            writer.AddAttribute(Constants.DataRel, rel);
            writer.RenderBeginTag(HtmlTextWriterTag.A);
            writer.Write(label);
            writer.RenderEndTag();
        }

        private void AddTitle(HtmlTextWriter writer, string title)
        {
            writer.RenderBeginTag(HtmlTextWriterTag.Title);
            writer.Write(title);
            writer.RenderEndTag();
        }

        private void AddMetaEncoding(HtmlTextWriter writer)
        {
            writer.WriteLine();
            writer.Write("<meta http-equiv=\"Content-Type\" content=\"text/html;charset=utf-8\" >");
            writer.WriteLine();
            writer.Write("<meta http-equiv=\"Pragma\" content=\"no-cache\" >");
        }

        private void AddMeta(HtmlTextWriter writer, string name, string content)
        {
            writer.WriteLine();
            if (!string.IsNullOrEmpty(name)) { writer.AddAttribute(HtmlTextWriterAttribute.Name, name); }
            if (!string.IsNullOrEmpty(content)) { writer.AddAttribute(HtmlTextWriterAttribute.Content, content); }
            writer.RenderBeginTag(HtmlTextWriterTag.Meta);
            writer.RenderEndTag();
        }

        private void AddLink(HtmlTextWriter writer, string rel, string href, string type)
        {
            writer.WriteLine();
            if (!string.IsNullOrEmpty(rel)) { writer.AddAttribute(HtmlTextWriterAttribute.Rel, rel); }
            if (!string.IsNullOrEmpty(href)) { writer.AddAttribute(HtmlTextWriterAttribute.Href, href); }
            if (!string.IsNullOrEmpty(type)) { writer.AddAttribute(HtmlTextWriterAttribute.Type, type); }
            writer.RenderBeginTag(HtmlTextWriterTag.Link);
            writer.RenderEndTag();
        }

        private void AddScript(HtmlTextWriter writer, string src)
        {
            writer.WriteLine();
            if (!string.IsNullOrEmpty(src)) { writer.AddAttribute(HtmlTextWriterAttribute.Src, src); }
            writer.RenderBeginTag(HtmlTextWriterTag.Script);
            writer.RenderEndTag();
        }

        private void AddFooter(HtmlTextWriter writer)
        {
            writer.AddAttribute(Constants.DataRole, Constants.FooterRole);
            writer.AddAttribute(Constants.DataPosition, Constants.PositionFixed);
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            AddFooterButtons(writer);
            writer.RenderEndTag(); 
        }

        private void AddInstructorPageFooter(HtmlTextWriter writer)
        {
            writer.AddAttribute(Constants.DataRole, Constants.FooterRole);
            writer.AddAttribute(Constants.DataPosition, Constants.PositionFixed);
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.RenderBeginTag(HtmlTextWriterTag.H3);
            AddMiniBtn(writer, "Instructor Sign-In", Constants.ClassDartUrl, string.Empty, "b", Constants.BlankTarget);
            writer.Write("&nbsp;");
            writer.Write("&nbsp;");
            writer.Write("&nbsp;");
            writer.Write("&nbsp;");
            AddClassDartLink(writer);
            writer.RenderEndTag();
            writer.RenderEndTag(); 
        }
        
        private void AddFooterButtons(HtmlTextWriter writer)
        {
            writer.RenderBeginTag(HtmlTextWriterTag.H3);
            AddBtnDialog(writer, "Subscribe to Class", "#" + Constants.SubscriptionId, Constants.None, "e");
            writer.Write("&nbsp;");
            writer.Write("&nbsp;");
            writer.Write("&nbsp;");
            writer.Write("&nbsp;");
            AddMiniBtn(writer, "Instructor Sign-In", Constants.ClassDartUrl, string.Empty, "b", Constants.BlankTarget);
            writer.Write("&nbsp;");
            writer.Write("&nbsp;");
            writer.Write("&nbsp;");
            writer.Write("&nbsp;");
            AddClassDartLink(writer);
            writer.RenderEndTag();
        }

        private void AddClassDartLink(HtmlTextWriter writer)
        {
            writer.Write("Created using ");
            writer.AddAttribute(HtmlTextWriterAttribute.Href, Constants.ClassDartUrl);
            writer.AddAttribute(HtmlTextWriterAttribute.Target, Constants.BlankTarget);
            writer.RenderBeginTag(HtmlTextWriterTag.A);
            writer.Write("Class Dart");
            writer.RenderEndTag();
        }

        private void AddTextArea(HtmlTextWriter writer, string columns, string theme, string name, string placeholder, string rows)
        {
            writer.AddAttribute(Constants.Columns, columns);
            writer.AddAttribute(Constants.DataTheme, theme);
            writer.AddAttribute(Constants.Name, name);
            writer.AddAttribute(Constants.Placeholder, placeholder);
            writer.AddAttribute(Constants.Rows, rows);
            writer.RenderBeginTag(HtmlTextWriterTag.Textarea);
            writer.RenderEndTag();
        }

        private void AddInputType(HtmlTextWriter writer, string type, string value, string name, string theme, string icon, string placeholder)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Type, type);
            writer.AddAttribute(HtmlTextWriterAttribute.Value, value);
            writer.AddAttribute(HtmlTextWriterAttribute.Name, name);
            if (!string.IsNullOrEmpty(placeholder)) { writer.AddAttribute(Constants.Placeholder, placeholder); }
            if (!string.IsNullOrEmpty(theme)) { writer.AddAttribute(Constants.DataTheme, theme); }
            if (!string.IsNullOrEmpty(icon)) { writer.AddAttribute(Constants.DataIcon, icon); }
            //writer.AddAttribute(Constants.DataInline, "true");
            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();
        }

        private void AddSubscriptionForm(HtmlTextWriter writer)
        {
            BeginPage(writer, Constants.SubscriptionId, string.Empty, "a");
            BeginHeader(writer, Constants.CornerTopClass, "a");
            writer.RenderBeginTag(HtmlTextWriterTag.H3);
            writer.Write("Subscribe");
            writer.RenderEndTag();
            writer.RenderEndTag();
            BeginContent(writer, "b");

            BeginForm(writer, SubscriptionUrl);
            writer.AddAttribute(HtmlTextWriterAttribute.Class, Constants.FormClass);
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            BeginFieldset(writer);
            AddInputType(writer, Constants.Hidden, Utilities.GetFullUrl(m_class.Url), "returnUrl", null, null, null);
            AddInputType(writer, Constants.Hidden, m_class.ID.ToString(), "classInput", null, null, null);
            writer.AddAttribute(HtmlTextWriterAttribute.Class, Constants.EditorFieldClass);
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.Write("Enter your email to get notified whenever announcements and assignments are added to this class.");
            writer.WriteBreak();
            writer.WriteBreak();
            AddInputType(writer, Constants.Text, null, "emailInput", "c", null, "Your email address");
            writer.RenderEndTag();
            writer.WriteBreak();
            AddBtnBack(writer, "Cancel", "#", "delete", "a", null);
            writer.WriteLine();
            AddInputType(writer, Constants.Submit, "Subscribe", "submitInput", "b", "check", null);
            writer.RenderEndTag();  //fieldset
            writer.RenderEndTag();  //div style
            writer.RenderEndTag();  //form
            writer.RenderEndTag();  //content
            writer.RenderEndTag();  //page
        }

        private void WriteToFile(string data, string filepath)
        {
            if (!Directory.Exists(filepath))
            {
                Directory.CreateDirectory(filepath);
            }

            using (StreamWriter outfile = new StreamWriter(filepath + Constants.OutputFile))
            {
                outfile.Write(data);
            }
        }

        #endregion private members

    }
}