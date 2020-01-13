using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AttendanceSystem.Models;
using AttendanceSystem.ViewModels;
using AttendanceSystem.Services;
using System.Text.Encodings.Web;
using AttendanceSystem.Data;
using AttendanceSystem.Repositories;
using Microsoft.Extensions.Configuration;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace AttendanceSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly ApplicationUserRepository applicationUserRepository;
        private readonly IEmailService emailService;
        private readonly IConfiguration configuration;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, AttendanceSystemContext attendanceSystemContext, IEmailService emailService, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.emailService = emailService;
            this.configuration = configuration;
            applicationUserRepository = new ApplicationUserRepository(attendanceSystemContext);
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await userManager.FindByEmailAsync(model.Email);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Sorry mate, your email is not recognized!");
                    return View(model);
                }

                if (!user.IsEnabled)    
                {
                    ModelState.AddModelError(string.Empty, "Sorry mate, your account has been disabled by the admin!");
                    return View(model);
                }

                SignInResult result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

                if (result.Succeeded)
                    return RedirectToAction("Index", "Home");

                ModelState.AddModelError(string.Empty, "Invalid Login Attempt: check email and password");
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult AccessDenied() => View();

        [HttpGet]
        public async Task<IActionResult> Details()
        {
            ApplicationUser user = await userManager.GetUserAsync(User);
            return View(new UserManagementModel(user, await userManager.IsInRoleAsync(user, ApplicationUser.AdminRole)));
        }

        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            return View(new UserManagementModel(await userManager.GetUserAsync(User)));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Manage(string id, UserManagementModel model)
        {
            ApplicationUser user = await userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            await applicationUserRepository.UpdateAsync(MapBasicInfo(user, model));

            TempData["StatusMessage"] = "Your profile info have been updated.";
            return RedirectToAction(nameof(Manage));
        }

        [HttpGet]
        public IActionResult ResetPassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await userManager.GetUserAsync(User);
                if (user == null)
                    return NotFound();

                IdentityResult changePasswordResult = await userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (changePasswordResult.Succeeded)
                {
                    await signInManager.RefreshSignInAsync(user);
                    TempData["StatusMessage"] = "Your password has been changed.";
                    return RedirectToAction(nameof(Manage));
                }

                foreach (IdentityError error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPasswordEmail()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPasswordEmail(string email)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await userManager.FindByEmailAsync(email);
                if (user == null )
                {
                    ModelState.AddModelError(string.Empty, "Sorry mate, your email is not recognized!");
                    return View();
                }

                if (!user.IsEnabled)
                {
                    ModelState.AddModelError(string.Empty, "Sorry mate, your account has been disabled by the admin!");
                    return View();
                }

                string token = await userManager.GeneratePasswordResetTokenAsync(user);
                string callbackUrl = Url.Action(
                    "ForgotPassword", 
                    "Account", 
                    values: new { token, email }, 
                    protocol: Request.Scheme,
                    configuration.GetValue<string>("HostUrl"));

                emailService.SendEmail(
                    email,
                    "Reset Password | Remal ATTEND",
                    "",
                    $"You asked for a forget password link, click the below button to reset your password",
                    "Reset Password",
                    HtmlEncoder.Default.Encode(callbackUrl));

                TempData["StatusMessage"] = "If provided email is correct, a reset password link has been sent.";
                return RedirectToAction(nameof(Login));
            }

            return View();
        }

        [HttpGet]
        public IActionResult ForgotPassword(string token, string email)
        {
            return View(new ForgotPasswordViewModel { Email = email, Token = token });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Sorry mate, your email is not recognized!");
                    return View();
                }

                IdentityResult result = await userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
                if (result.Succeeded)
                {
                    TempData["StatusMessage"] = "Password has been reset.";
                    return RedirectToAction(nameof(Login));
                }
            }

            return View(model);
        }

        private ApplicationUser MapBasicInfo(ApplicationUser user, UserManagementModel model)
        {
            user.Email = model.Email;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            return user;
        }
    }
}