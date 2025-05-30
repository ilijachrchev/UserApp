﻿using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices.Marshalling;

namespace UserApp.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "FirstName is required.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "LastName is required.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "LastName is required.")]
        public string AccUserName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Date Of Birth is required.")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        
        public string? Sex { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Password is required!")]
        [StringLength(40, MinimumLength = 8, ErrorMessage = "The {0} must be at {2} and at max {1} character long!")]
        [DataType(DataType.Password)] 
        [Compare("ConfirmPassword", ErrorMessage = "Password does not match!")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required.")]
        [DataType(DataType.Password)]

        [Display(Name = "Confirm Password")]

        public string ConfirmPassword { get; set; }
    }
}
