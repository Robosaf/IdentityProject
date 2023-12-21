using IdentityProject.Data;
using IdentityProject.Interfaces;
using IdentityProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityProject.Controllers
{
    public class RoleController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRoleRepository _userRoleRepository;

        public RoleController(ApplicationDbContext applicationDbContext, 
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IRoleRepository roleRepository,
            IUserRoleRepository userRoleRepository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {

            var roles = _roleRepository.GetAllRoles();
            return View(roles);
        }

        public async Task<IActionResult> Upsert(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return View();
            }
            else
            {
                var role = _roleRepository.GetRoleById(id);
                return View(role);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(IdentityRole identityRole)
        {
            if (await _roleManager.RoleExistsAsync(identityRole.Name))
            {
                return RedirectToAction("Index");
            }

            if (string.IsNullOrEmpty(identityRole.Id))
            {
                await _roleManager.CreateAsync(new IdentityRole { Name = identityRole.Name });
            }
            else
            {
                var roleDb = _roleRepository.GetRoleById(identityRole.Id);

                if (roleDb == null)
                {
                    return RedirectToAction(nameof(Index));
                }

                roleDb.Name = identityRole.Name;
                roleDb.NormalizedName = identityRole.Name.ToUpper();

                var result = await _roleManager.UpdateAsync(roleDb);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var role = _roleRepository.GetRoleById(id);

            if (role == null)
            {
                return RedirectToAction(nameof(Index));
            }
            var userRolesForThisRole = _userRoleRepository.GetUserRolesByRoleId(id).Count;

            if (userRolesForThisRole > 0)
            {
                return RedirectToAction(nameof(Index));
            }

            await _roleManager.DeleteAsync(role);
            return RedirectToAction(nameof(Index));
        }
    }
}
