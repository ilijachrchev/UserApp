using Microsoft.AspNetCore.Identity;

namespace UserApp.Models
{
    public class Users : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; } // This property is inherited from IdentityUser
        public DateTime DateOfBirth { get; set; }
        public string Sex { get; set; }
    }
}
