using IdentityProject.Interfaces;
using IdentityProject.Models;
using IdentityProject.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace IdentityProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;

        public AccountController(UserManager<AppUser> userManager, 
            SignInManager<AppUser> signInManager, IEmailSender emailSender,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _roleManager = roleManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Register(string? returnUrl = null)
        {
            if (!await _roleManager.RoleExistsAsync("Car"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Car"));
                await _roleManager.CreateAsync(new IdentityRole("AutoEngineer"));
            }

            List<SelectListItem> selectedItems = new List<SelectListItem>();
            selectedItems.Add(new SelectListItem()
            {
                Value = "Car",
                Text = "Car"
            });
            selectedItems.Add(new SelectListItem()
            {
                Value = "AutoEngineer",
                Text = "AutoEngineer"
            });


            RegisterViewModel registerViewModel = new RegisterViewModel
            { 
                ReturnUrl = returnUrl,
                RoleList = selectedItems
            };

            return View(registerViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel, string? returnUrl)
        {
            registerViewModel.ReturnUrl = returnUrl;
            returnUrl = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                var user = new AppUser { Email = registerViewModel.Email, UserName = registerViewModel.Email, NickName = registerViewModel.UserName};
                var result = await _userManager.CreateAsync(user, registerViewModel.Password);

                if (result.Succeeded)
                {
                    if (registerViewModel.RoleSelected != null && registerViewModel.RoleSelected.Length > 0
                        && registerViewModel.RoleSelected == "AutoEngineer")
                    {
                        await _userManager.AddToRoleAsync(user, "AutoEngineer");
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(user, "Car");
                    }
                    
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
                ModelState.AddModelError("Password", "User could not be created. Password not unique enough");
            }

            return View(registerViewModel);
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            LoginViewModel loginViewModel = new LoginViewModel()
            {
                ReturnUrl = returnUrl ?? Url.Content("~/")
            };

            return View(loginViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel, string? returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = await _signInManager.PasswordSignInAsync(loginViewModel.Email, 
                    loginViewModel.Password, loginViewModel.RememberMe, lockoutOnFailure: true);
                if (user.Succeeded) 
                {
                    return RedirectToAction("Index", "Home");
                }
                if (user.IsLockedOut)
                {
                    return View("Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attemp");
                    return View(loginViewModel);
                }
            }

            return View(loginViewModel);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnURL = null)
        {
            var redirect = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnURL });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirect);

            return Challenge(properties, provider);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string returnURL = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, "Error from external provider");
                return View("Login");
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            
            if (info == null)
            {
                return RedirectToAction("Login");
            }

            if (await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey) == null)
            {

                // Gets an email from external claims
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                // Checks whether the account already exists
                // Uses the email address as a common link among multiple providers
                var user = await _userManager.FindByEmailAsync(email);
                var login = new Microsoft.AspNetCore.Identity.UserLoginInfo(info.LoginProvider, info.ProviderKey, null);

                if (user != null)
                {
                    // If the account already exists the visitor is using a different
                    // external provider. Bind the new external login to an existing account.
                    await _userManager.AddLoginAsync(user, login);
                }
            }

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);

            if (result.Succeeded)
            {
                await _signInManager.UpdateExternalAuthenticationTokensAsync(info);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewData["ReturnUrl"] = returnURL;
                ViewData["ProviderDisplayName"] = info.ProviderDisplayName;
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                return View("ExternalLoginConfirmation", new ExternalLoginViewModel { Email = email });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginViewModel model, string? returnURL = null)
        {
            returnURL = returnURL ?? Url.Content("~/");

            if(ModelState.IsValid)
            {
                var info = await _signInManager.GetExternalLoginInfoAsync();

                if (info == null)
                {
                    return View("Error");
                }

                var user = new AppUser { UserName = model.Email, Email = model.Email, NickName = model.Name };
                var result = await _userManager.CreateAsync(user);

                if (result.Succeeded)
                {
                    //await _userManager.AddToRoleAsync(user, "User");
                    result = await _userManager.AddLoginAsync(user, info);

                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        await _signInManager.UpdateExternalAuthenticationTokensAsync(info);

                        return LocalRedirect(returnURL);
                    }
                }

                ModelState.AddModelError("Email", "User already exists");
            }
            ViewData["ReturnUrl"] = returnURL;
            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel forgotPasswordViewModel)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(forgotPasswordViewModel.Email);

                if (user == null)
                {
                    return RedirectToAction("ForgotPasswordConfirmation");
                }

                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackURL = Url.Action("ResetPassword", "Account",
                    new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);

                await _emailSender.SendEmailAsync(forgotPasswordViewModel.Email, "Reset Email Confirmation",
                    $"Please reset email by going this <a href =\"{callbackURL}\">link</a>");

                return RedirectToAction("ForgotPasswordConfirmation");
            }

            return View(forgotPasswordViewModel);
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }


        [HttpGet]
        public IActionResult ResetPassword(string code = null)
        {
            
            return code == null ? View("Error") : View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPasswordViewModel)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(resetPasswordViewModel.Email);

                if (user == null)
                {
                    ModelState.AddModelError("Email", "User not found");
                    return View();
                }

                var result = await _userManager.ResetPasswordAsync(user, 
                    resetPasswordViewModel.Code, resetPasswordViewModel.Password);

                if (result.Succeeded)
                {
                    return RedirectToAction("ResetPasswordConfirmation");
                }
            }

            return View(resetPasswordViewModel);
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
