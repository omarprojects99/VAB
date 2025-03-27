using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VAB.ViewModel;
using VAB.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace VAB.Controllers
{
    [Authorize(Roles ="Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public AdminController(UserManager<User> userManager,RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }
        
       
        public IActionResult Dashboard()
        {
            return View();
        }
        public async Task<IActionResult> UsersListAsync()
        {
            var users = _userManager.Users.ToList();
            var userList = new List<UserManagementViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(new UserManagementViewModel
                {   Id= user.Id,
                    NomPrenom = user.NomPrenom,
                    Email = user.Email,
                    Telephone = user.Telephone,
                    Roles = roles.ToList()
                });
            }

            return View(userList);
        }
        public IActionResult AddUser()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUser(UserManagementViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                Telephone = model.Telephone,
                NomPrenom = model.NomPrenom,
                IsActive = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                EmailConfirmed = true,
                LockoutEnabled = true,
            };

            var defaultPassword = "[REDACTED]"; // Update this for production
            var result = await _userManager.CreateAsync(user, defaultPassword);

            if (result.Succeeded)
            {
                // Check if Role exists
                var roleExists = await _roleManager.RoleExistsAsync(model.Role);
                if (!roleExists)
                {
                    ModelState.AddModelError("", "❌ Role does not exist!");
                    return View(model);
                }

                // Assign Role
                try
                {
                    var roleResult = await _userManager.AddToRoleAsync(user, model.Role);
                    if (!roleResult.Succeeded)
                    {
                        foreach (var error in roleResult.Errors)
                        {
                            Console.WriteLine($"Error assigning role: {error.Description}");
                        }
                        ModelState.AddModelError("", "❌ Failed to assign role!");
                        return View(model);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                    ModelState.AddModelError("", "❌ An unexpected error occurred while assigning the role.");
                    return View(model);
                }

                // Debug the assigned role
                var roles = await _userManager.GetRolesAsync(user);
                Console.WriteLine("✅ User roles: " + string.Join(", ", roles));

                TempData["Success"] = "✅ User created and role assigned successfully";
                return RedirectToAction("UsersList");
            }

            // Display Identity errors
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("UsersList");
            }

            return BadRequest("Error deleting user");
        }

        public async Task<IActionResult> ViewUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);

            var model = new UserManagementViewModel
            {
                Id = user.Id,
                NomPrenom = user.NomPrenom,
                Email = user.Email,
                Telephone = user.Telephone,
                Roles = roles.ToList()
            };

            return View(model);
        }

        public async Task<IActionResult> EditUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var availableRoles = _roleManager.Roles.Select(r => r.Name).ToList(); // Fetch all available roles

            var model = new UserManagementViewModel
            {
                Id = user.Id,
                NomPrenom = user.NomPrenom,
                Email = user.Email,
                Telephone = user.Telephone,
                Roles = roles.ToList(),
                AvailableRoles = availableRoles
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> EditUserPost(UserManagementViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("EditUser", model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return NotFound();
            }

            user.NomPrenom = model.NomPrenom;
            user.Email = model.Email;
            user.Telephone = model.Telephone;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                ModelState.AddModelError("", "Failed to update user.");
                return View("EditUser", model);
            }

            // Update user roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles); // Remove old roles
            await _userManager.AddToRolesAsync(user, model.Roles); // Add new roles

            return RedirectToAction("UsersList");
        }

    }




}


