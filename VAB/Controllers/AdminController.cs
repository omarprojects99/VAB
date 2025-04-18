using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VAB.ViewModel;
using VAB.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using VAB.Data;

namespace VAB.Controllers
{
    [Authorize(Roles ="Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        public AdminController(UserManager<User> userManager,RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
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

        [HttpGet]
        public async Task<IActionResult> AccountSettings()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login"); // Redirect if not logged in
            }

            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user == null)
            {
                return RedirectToAction("Login"); // Redirect if user not found
            }

            var roles = await _userManager.GetRolesAsync(user); // Get user roles

            var model = new UserManagementViewModel
            {
                NomPrenom = user.NomPrenom,
                Email = user.Email,
                Telephone = user.Telephone,
                Role = roles.FirstOrDefault() ?? "User" // Get first role or default to 'User'
            };

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> UpdateProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var model = new UserManagementViewModel
            {
                NomPrenom = user.NomPrenom,
                Email = user.Email,
                Telephone = user.Telephone
            };

            return View("UpdateProfile", model);
        }
        //categorie
        public async Task<IActionResult> Categories()
        {
            var categories = await _context.Categories.ToListAsync();
            return View(categories);
        }
        // Show all categories

        // GET: Add Category
        public IActionResult AddCategorie()
        {
            return View();
        }

        // POST: Add Category
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategorie(CategorieManagementViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var categorie = new Categorie
                    {
                        Description = model.Description
                        // You can initialize other default values here if needed
                    };

                    _context.Add(categorie);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Categories));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("SQL ERROR: " + ex.Message);
                    ModelState.AddModelError(string.Empty, "An error occurred while saving. Try again.");
                }
            }

            return View(model);
        }
        //Edit
        // GET: Edit Categorie
        public async Task<IActionResult> EditCategorie(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            // Find the category by id
            var categorie = await _context.Categories.FindAsync(id);
            if (categorie == null)
            {
                return NotFound();
            }

            // Map to the view model
            var model = new CategorieManagementViewModel
            {
                Id = categorie.Id,
                Description = categorie.Description
            };

            // Return the view with the category data
            return View(model);
        }

        // POST: Edit Categorie
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategorie(CategorieManagementViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var categorie = await _context.Categories.FindAsync(model.Id);

                    if (categorie == null)
                    {
                        return NotFound();
                    }

                    // Update only the Description field
                    categorie.Description = model.Description;

                    // Save changes
                    _context.Update(categorie);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Categories)); // Redirect back to Categories
                }
                catch (Exception ex)
                {
                    Console.WriteLine("SQL ERROR: " + ex.Message);
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the category. Please try again.");
                }
            }

            return View(model); // Return the view with the model if validation fails
        }






    }




}


