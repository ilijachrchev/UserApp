using Microsoft.AspNetCore.Mvc;
using UserApp.ViewModels;
using UserApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using UserApp.Services;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using UserApp.Data;
using Microsoft.EntityFrameworkCore;


namespace UserApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : Controller
    {
        private readonly SignInManager<Users> signInManager;
        private readonly UserManager<Users> userManager;
        private readonly IEmailSender emailSender;
        private readonly AppDbContext _context;


        public AccountController(SignInManager<Users> signInManager, UserManager<Users> userManager, IEmailSender emailSender, AppDbContext context)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.emailSender = emailSender;
            this._context = context;
            
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // determine if the user is using username or email
                var userNameOrEmail = model.UserNameOrEmail;
                string actualUserName = userNameOrEmail;

                // chek if input is email format firstly
                if (userNameOrEmail.Contains("@"))
                {
                    // validating email format
                    var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                    if (!emailRegex.IsMatch(userNameOrEmail))
                    {
                        ModelState.AddModelError("UserNameOrEmail", "Invalid email format.");
                        return View(model);
                    }
                    // finding username by email
                    var user = await userManager.FindByEmailAsync(userNameOrEmail);
                    if (user == null) {
                        ModelState.AddModelError("", "Invalid login attempt.");
                        return View(model);
                    }
                    actualUserName = user.UserName;
                }
                else
                {
                    // validating username format (only alphanumeric characters)
                    var usernameRegex = new Regex(@"^[a-zA-Z0-9]+$");
                    if (!usernameRegex.IsMatch(userNameOrEmail))
                    {
                        ModelState.AddModelError("UserNameOrEmail", "Invalid username format.");
                        return View(model);
                    }
                }

                // attempt login with determined username
                var result = await signInManager.PasswordSignInAsync(
                    actualUserName,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }

                // general error message for security
                ModelState.AddModelError("", "Invalid login attempt.");
            }
            return View(model);
        }

        public IActionResult Register()
        {
            return View();
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { errors });
            }

            // Check user's age
            var today = DateTime.Today;
            var age = today.Year - model.DateOfBirth.Year;
            if (model.DateOfBirth > today.AddYears(-age)) age--;
            if (age < 1)
            {
                return BadRequest(new { error = "You must be at least 12 years old to register." });
            }

            Users users = new Users
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                AccUserName = model.AccUserName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                DateOfBirth = model.DateOfBirth,
                Sex = model.Sex,
                UserName = model.Email
            };

            var result = await userManager.CreateAsync(users, model.Password);

            if (result.Succeeded)
            {
                return Ok(new { message = "Registration successful!" });
            }
            else
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { errors });
            }
        }


        public IActionResult VerifyEmail()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);

                if (user == null)
                {
                    ModelState.AddModelError("", "Something is wrong!");
                    return View(model);
                }
                else
                {
                    return RedirectToAction("ChangePassword", "Account", new { username = user.UserName });
                }
            }
            return View(model);
        }

        public IActionResult ChangePassword(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("VerifyEmail", "Account");
            }
            return View(new ChangePasswordViewModel { Email = username });
        }
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByNameAsync(model.Email);
                if (user != null)
                {
                    var result = await userManager.RemovePasswordAsync(user);
                    if (result.Succeeded)
                    {
                        result = await userManager.AddPasswordAsync(user, model.NewPassword);
                        return RedirectToAction("Login", "Account");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                        return View(model);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Email not found!");
                    return View(model);
                }
            }
            else
            {
                ModelState.AddModelError("", "Something went wrong. Try Again!");
                return View(model);
            }
        }

        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new ProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> SendEmailConfirmation()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (user.EmailConfirmed)
            {
                TempData["StatusMessage"] = "Your Email is already confirmed.";
                return RedirectToAction("Profile");
            }

            // Generate the email confirmation token
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

            // Generete the confirmation link
            var confirmationLink = Url.Action(
                "ConfirmEmail",
                "Account",
                new { userId = user.Id, token = token },
                Request.Scheme);

            // Send the email (using your email service)
            var emailSent = await emailSender.SendEmailAsync(
                user.Email,
                "Please Confirm Your Email",
                $"Please Confirm Your Email By Clicking This Link: <a href='{confirmationLink}'>Confirm Email</a>");

            if (emailSent)
            {
                TempData["StatusMessage"] = "Confirmation email sent. Please check your inbox.";
            }
            else
            {
                TempData["StatusMessage"] = "Error sending confirmation email. Please try again.";
            }
            return RedirectToAction("Profile");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                return RedirectToAction("Home", "Account");
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View("Error");
            }

            var result = await userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                return View("ConfirmEmail"); // with ConfirmEmail.cshtml i will show success message
            }
            else
            {
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> TestEmail()
        {
            var result = await emailSender.SendEmailAsync(
                "your_other_email@example.com", // ✅ Replace this with any email you have access to
                "Test Email from UserApp",
                "<p>This is a test email sent from the AccountController.</p>"
            );

            if (result)
                return Content("✅ Email sent successfully!");
            else
                return Content("❌ Email sending failed.");
        }

        [HttpPost]
        public async Task<IActionResult> SendPhoneVerification(ProfileViewModel model)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var token = await userManager.GenerateChangePhoneNumberTokenAsync(user, model.PhoneNumber);

            TempData["StatusMessage"] = "Phone verification code sent. Please check your phone!";
            return RedirectToAction("Profile");
        }

        [HttpPost]
        public async Task<IActionResult> VerifyPhone(ProfileViewModel model)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var result = await userManager.ChangePhoneNumberAsync(user, model.PhoneNumber, model.PhoneVerificationCode);
            if (result.Succeeded)
            {
                TempData["StatusMessage"] = "Phone number verified successfully.";
                return RedirectToAction("Profile");
            }
            ModelState.AddModelError("", "Invalid verification code.");
            return View("Profile", model);
        }

        [HttpGet]
        public JsonResult CheckEmailAvailability(string email)
        {
            var isEmailTaken = _context.Users.Any(u => u.Email == email);
            return Json(new { available = !isEmailTaken });
        }


    }
}
