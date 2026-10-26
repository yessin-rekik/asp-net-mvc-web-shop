using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using tp2.ViewModels;

namespace tp2.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser
                {
                    UserName = model.Email,
                    Email = model.Email
                };

                var result = await userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if (await roleManager.RoleExistsAsync("Client"))
                    {
                        await userManager.AddToRoleAsync(user, "Client");
                    }

                    await signInManager.SignInAsync(user, isPersistent: false);
                    TempData["SuccessMessage"] = "Bienvenue ! Votre compte a été créé.";
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl)
        {
            if (ModelState.IsValid)
            {
                var result = await signInManager.PasswordSignInAsync(
                    model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return LocalRedirect(returnUrl);
                    }

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "E-mail ou mot de passe incorrect.");
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            await LoadProfileRolesAsync(user);
            return View(new ProfileViewModel { Email = user.Email ?? string.Empty });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            await LoadProfileRolesAsync(user);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var currentEmail = user.Email ?? string.Empty;
            if (string.Equals(currentEmail, model.Email, StringComparison.OrdinalIgnoreCase))
            {
                TempData["SuccessMessage"] = "Vos informations sont déjà à jour.";
                return RedirectToAction(nameof(Profile));
            }

            var existing = await userManager.FindByEmailAsync(model.Email);
            if (existing != null && existing.Id != user.Id)
            {
                ModelState.AddModelError(nameof(model.Email), "Cette adresse e-mail est déjà utilisée.");
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.CurrentPassword))
            {
                ModelState.AddModelError(nameof(model.CurrentPassword), "Saisissez votre mot de passe actuel pour changer d'adresse e-mail.");
                return View(model);
            }

            if (!await userManager.CheckPasswordAsync(user, model.CurrentPassword))
            {
                ModelState.AddModelError(nameof(model.CurrentPassword), "Le mot de passe actuel est incorrect.");
                return View(model);
            }

            var emailResult = await userManager.SetEmailAsync(user, model.Email);
            if (!emailResult.Succeeded)
            {
                AddIdentityErrors(emailResult);
                return View(model);
            }

            var userNameResult = await userManager.SetUserNameAsync(user, model.Email);
            if (!userNameResult.Succeeded)
            {
                AddIdentityErrors(userNameResult);
                return View(model);
            }

            await signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "Votre adresse e-mail a été mise à jour.";
            return RedirectToAction(nameof(Profile));
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                AddIdentityErrors(result);
                return View(model);
            }

            await signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "Votre mot de passe a été modifié.";
            return RedirectToAction(nameof(Profile));
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private async Task LoadProfileRolesAsync(IdentityUser user)
        {
            ViewBag.Roles = await userManager.GetRolesAsync(user);
        }

        private void AddIdentityErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
    }
}
