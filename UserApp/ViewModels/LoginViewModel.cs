using System.ComponentModel.DataAnnotations;

namespace UserApp.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username/Email is required")]
        [Display(Name = "Username/Email")]
        public string UserNameOrEmail { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
