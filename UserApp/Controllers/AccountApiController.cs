using Microsoft.AspNetCore.Mvc;
using UserApp.ViewModels;
using UserApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using UserApp.Services;
using UserApp.Data;
using System.Text.RegularExpressions;

namespace UserApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountApiController : ControllerBase
    {
        private readonly SignInManager<Users> signInManager;
        private readonly UserManager<Users> userManager;
        private readonly IEmailSender emailSender;
        private readonly AppDbContext _context;

        public AccountApiController(SignInManager<Users> signInManager, UserManager<Users> userManager, IEmailSender emailSender, AppDbContext context)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.emailSender = emailSender;
            this._context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check age
            var today = DateTime.Today;
            var age = today.Year - model.DateOfBirth.Year;
            if (model.DateOfBirth > today.AddYears(-age)) age--;
            if (age < 12)
            {
                return BadRequest(new { error = "You must be at least 12 years old to register." });
            }

            var user = new Users
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                DateOfBirth = model.DateOfBirth,
                Sex = model.Sex,
                UserName = model.Email
            };

            var result = await userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // optional: send confirmation email if you want
                return Ok(new { message = "Registration successful!" });
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string actualUserName = model.UserNameOrEmail;

            if (model.UserNameOrEmail.Contains("@"))
            {
                var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                if (!emailRegex.IsMatch(model.UserNameOrEmail))
                {
                    return BadRequest(new { error = "Invalid email format." });
                }

                var user = await userManager.FindByEmailAsync(model.UserNameOrEmail);
                if (user == null)
                {
                    return Unauthorized(new { error = "Invalid login attempt." });
                }
                actualUserName = user.UserName;
            }
            else
            {
                var usernameRegex = new Regex(@"^[a-zA-Z0-9]+$");
                if (!usernameRegex.IsMatch(model.UserNameOrEmail))
                {
                    return BadRequest(new { error = "Invalid username format." });
                }
            }

            var result = await signInManager.PasswordSignInAsync(actualUserName, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return Ok(new { message = "Login successful." });
            }

            return Unauthorized(new { error = "Invalid login attempt." });
        }
    }
}
