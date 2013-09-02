using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ClassDart.Models
{

    public class HomeViewModel
    {
        public SignInViewModel SignInViewModel { get; set; }
        public NewUserViewModel NewUserViewModel { get; set; }
        public CreateClassViewModel CreateClassViewModel { get; set; }
    }

    public class SignInViewModel
    {
        [Display(Name = "User name")]
        [Required(ErrorMessage = "Your email is required as your user name.")]
        public string UserName { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [Required(ErrorMessage = "Your password is required.")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class NewUserViewModel
    {
        [Display(Name = "First name")]
        [Required(ErrorMessage = "Your first name is required.")]
        public string FirstName { get; set; }

        [Display(Name = "Last name")]
        [Required(ErrorMessage = "Your last name is required.")]
        public string LastName { get; set; }

        [Display(Name = "User name")]
        [Required(ErrorMessage = "Your email is required.")]
        public string UserName { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [Required(ErrorMessage = "Your password is required.")]
        public string Password { get; set; }
    }

    public class CreateClassViewModel
    {
        [Display(Name = "Name of class")]
        [Required(ErrorMessage = "Your class name is required.")]
        public string ClassName { get; set; }
    }
}