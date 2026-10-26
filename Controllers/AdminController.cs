using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using tp2.ViewModels;

namespace tp2.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<IdentityUser> userManager;

        public AdminController(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
        }

        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            IdentityRole role = new IdentityRole { Name = model.RoleName };
            IdentityResult result = await roleManager.CreateAsync(role);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Le rôle a été créé.";
                return RedirectToAction(nameof(ListRoles));
            }

            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ListRoles()
        {
            var roles = roleManager.Roles.OrderBy(r => r.Name);
            return View(roles);
        }

        [HttpGet]
        public async Task<IActionResult> EditRole(string id)
        {
            var role = await roleManager.FindByIdAsync(id);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Le rôle avec l'identifiant {id} est introuvable.";
                return View("NotFound");
            }

            var model = new EditRoleViewModel
            {
                Id = role.Id,
                RoleName = role.Name ?? string.Empty
            };

            foreach (var user in userManager.Users.ToList())
            {
                if (await userManager.IsInRoleAsync(user, role.Name!))
                {
                    model.Users.Add(user.UserName ?? user.Email ?? user.Id);
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRole(EditRoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var role = await roleManager.FindByIdAsync(model.Id);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Le rôle avec l'identifiant {model.Id} est introuvable.";
                return View("NotFound");
            }

            role.Name = model.RoleName;
            var result = await roleManager.UpdateAsync(role);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Le rôle a été mis à jour.";
                return RedirectToAction(nameof(ListRoles));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await roleManager.FindByIdAsync(id);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Le rôle avec l'identifiant {id} est introuvable.";
                return View("NotFound");
            }

            var usersInRole = await userManager.GetUsersInRoleAsync(role.Name!);
            if (usersInRole.Any())
            {
                TempData["ErrorMessage"] = "Impossible de supprimer un rôle encore attribué à des utilisateurs.";
                return RedirectToAction(nameof(ListRoles));
            }

            var result = await roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Le rôle a été supprimé.";
                return RedirectToAction(nameof(ListRoles));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return RedirectToAction(nameof(ListRoles));
        }

        [HttpGet]
        public async Task<IActionResult> EditUsersInRole(string roleId)
        {
            ViewBag.roleId = roleId;
            var role = await roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Le rôle avec l'identifiant {roleId} est introuvable.";
                return View("NotFound");
            }

            var model = new List<UserRoleViewModel>();
            foreach (var user in userManager.Users.ToList())
            {
                var userRoleViewModel = new UserRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName ?? user.Email ?? user.Id,
                    IsSelected = await userManager.IsInRoleAsync(user, role.Name!)
                };
                model.Add(userRoleViewModel);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUsersInRole(List<UserRoleViewModel> model, string roleId)
        {
            var role = await roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"Le rôle avec l'identifiant {roleId} est introuvable.";
                return View("NotFound");
            }

            foreach (var item in model)
            {
                var user = await userManager.FindByIdAsync(item.UserId);
                if (user == null)
                {
                    continue;
                }

                var isInRole = await userManager.IsInRoleAsync(user, role.Name!);
                if (item.IsSelected && !isInRole)
                {
                    var addResult = await userManager.AddToRoleAsync(user, role.Name!);
                    if (!addResult.Succeeded)
                    {
                        TempData["ErrorMessage"] = string.Join(" ", addResult.Errors.Select(e => e.Description));
                    }
                }
                else if (!item.IsSelected && isInRole)
                {
                    var removeResult = await userManager.RemoveFromRoleAsync(user, role.Name!);
                    if (!removeResult.Succeeded)
                    {
                        TempData["ErrorMessage"] = string.Join(" ", removeResult.Errors.Select(e => e.Description));
                    }
                }
            }

            TempData["SuccessMessage"] = "Les utilisateurs du rôle ont été mis à jour.";
            return RedirectToAction(nameof(EditRole), new { Id = roleId });
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
