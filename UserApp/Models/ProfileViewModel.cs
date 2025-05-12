namespace UserApp.Models
{
    public class ProfileViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public string PhoneVerificationCode { get; set; } //for user input of code
        public string Address { get; set; }
        public string ProfilePictureUrl { get; set; }
        public bool EmailConfirmed { get; set; }
    }


}
