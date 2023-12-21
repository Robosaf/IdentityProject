using IdentityProject.Data;
using IdentityProject.Interfaces;
using IdentityProject.Models;
using IdentityProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace IdentityProject.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IRoleRepository _roleRepository;

        public UserController(UserManager<AppUser> userManager,
            ApplicationDbContext applicationDbContext,
            IUserRepository userRepository,
            IUserRoleRepository userRoleRepository,
            IRoleRepository roleRepository)
        {
            _userManager = userManager;
            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;
            _roleRepository = roleRepository;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userList = _userRepository.GetAllAppUsers();
            var userRole = _userRoleRepository.GetAllUserRoles();
            var roles = _roleRepository.GetAllRoles();

            foreach (var user in userList)
            {
                
                var role = userRole.FirstOrDefault(u => u.UserId == user.Id);

                if (role == null)
                {
                    user.Role = "None";
                }
                else
                {
                    user.Role = roles.FirstOrDefault(u => u.Id == role.RoleId).Name;
                }

            }

            return View(userList);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string userId)
        {

            var user = _userRepository.GetAppUserById(userId);

            if (user == null)
            {
                return NotFound();
            }

            var userRole = _userRoleRepository.GetAllUserRoles();
            var roles = _roleRepository.GetAllRoles();
            var role = userRole.FirstOrDefault(u => u.UserId == user.Id);

            if (userRole != null)
            {
                user.RoleId = roles.FirstOrDefault(u => u.Id == role.RoleId).Id;
            }

            user.RoleList = _roleRepository.GetAllRoles().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id
            });


            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AppUser user)
        {
            if (ModelState.IsValid)
            {
                var userDbValue = _userRepository.GetAppUserById(user.Id);

                if (userDbValue == null)
                {
                    return NotFound();
                }

                var userRole = _userRoleRepository.GetUserRoleByUserId(userDbValue.Id);

                if (userRole != null)
                {
                    var previousRoleName = _roleRepository.GetRoleById(userRole.RoleId).Name;

                    await _userManager.RemoveFromRoleAsync(userDbValue, previousRoleName);
                }

                await _userManager.AddToRoleAsync(userDbValue, _roleRepository.GetRoleById(user.RoleId).Name);
                _userRepository.Save();

                return RedirectToAction(nameof(Index));
            }

            user.RoleList = _roleRepository.GetAllRoles().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id
            });


            return View(user);
        }


        [HttpPost]
        public async Task<IActionResult> Delete(string userId)
        {
            var user = _userRepository.GetAppUserById(userId);

            if (user == null)
            {
                return NotFound();
            }

            _userRepository.DeleteAppUser(user);

            return RedirectToAction(nameof(Index));
        }
    }
}
