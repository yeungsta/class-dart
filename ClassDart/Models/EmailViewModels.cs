using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClassDart.Models
{
    public class SendSubscribedEmailViewModel
    {
        public string Email { get; set; }
        public string ClassName { get; set; }
        public string UnsubscribeLink { get; set; }
    }

    public class SendClassUpdateEmailViewModel
    {
        public string Email { get; set; }
        public string ClassName { get; set; }
        public string ClassLink { get; set; }
        public string UnsubscribeLink { get; set; }
    }
}