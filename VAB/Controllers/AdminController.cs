using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VAB.ViewModel;
using VAB.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using VAB.Data;
using System.Linq;

namespace VAB.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public AdminController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // Users List
        public async Task<IActionResult> UsersList()
        {
            var users = _userManager.Users.ToList();
            var userList = new List<UserManagementViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(new UserManagementViewModel
                {
                    Id = user.Id,
                    NomPrenom = user.NomPrenom,
                    Email = user.Email,
                    Telephone = user.Telephone,
                    Roles = roles.ToList(),
                    IsActive = user.IsActive
                });
            }

            return View(userList);
        }

        // Add User GET
        public IActionResult AddUser()
        {
            return View();
        }

        // Add User POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUser(UserManagementViewModel model)
        {
            try
            {
                Console.WriteLine($"AddUser called with: {model.NomPrenom}, {model.Email}, {model.Telephone}, {model.Role}");

                // IMPORTANT: Remove Password from ModelState validation for AddUser operation
                ModelState.Remove("Password");

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    Console.WriteLine($"ModelState errors: {string.Join(", ", errors)}");
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                // Enhanced validation
                if (!IsValidPhoneNumber(model.Telephone))
                {
                    Console.WriteLine("Phone validation failed");
                    return Json(new { success = false, message = "Le numéro de téléphone doit contenir exactement 8 chiffres" });
                }

                if (!IsValidEmail(model.Email))
                {
                    Console.WriteLine("Email validation failed");
                    return Json(new { success = false, message = "Veuillez entrer une adresse email valide" });
                }

                // Validate Role is provided
                if (string.IsNullOrEmpty(model.Role))
                {
                    Console.WriteLine("Role validation failed");
                    return Json(new { success = false, message = "Le rôle est requis" });
                }

                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    Console.WriteLine("User already exists");
                    return Json(new { success = false, message = "Un utilisateur avec cet email existe déjà" });
                }

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

                var defaultPassword = GenerateSecurePassword();
                Console.WriteLine($"Creating user with password: {defaultPassword}");

                var result = await _userManager.CreateAsync(user, defaultPassword);

                if (result.Succeeded)
                {
                    Console.WriteLine("User created successfully");

                    // Check if Role exists
                    var roleExists = await _roleManager.RoleExistsAsync(model.Role);
                    if (!roleExists)
                    {
                        Console.WriteLine($"Role {model.Role} does not exist");
                        await _userManager.DeleteAsync(user); // Cleanup
                        return Json(new { success = false, message = "Le rôle sélectionné n'existe pas" });
                    }

                    // Assign Role
                    var roleResult = await _userManager.AddToRoleAsync(user, model.Role);
                    if (!roleResult.Succeeded)
                    {
                        Console.WriteLine("Failed to assign role");
                        await _userManager.DeleteAsync(user); // Cleanup
                        return Json(new { success = false, message = "Échec de l'attribution du rôle à l'utilisateur" });
                    }

                    // Set TempData for success message
                    TempData["Success"] = $"✅ Utilisateur '{model.NomPrenom}' créé avec succès! Mot de passe par défaut: {defaultPassword}";

                    Console.WriteLine("User creation completed successfully");
                    return Json(new
                    {
                        success = true,
                        message = "Utilisateur créé avec succès",
                        defaultPassword = defaultPassword
                    });
                }

                var errorMessages = result.Errors.Select(e => e.Description);
                Console.WriteLine($"User creation failed: {string.Join(", ", errorMessages)}");
                return Json(new { success = false, message = string.Join(", ", errorMessages) });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in AddUser: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new { success = false, message = "Une erreur inattendue s'est produite lors de la création de l'utilisateur" });
            }
        }

        // View User
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
                Roles = roles.ToList(),
                IsActive = user.IsActive
            };

            return View(model);
        }

        // Edit User GET
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
            var availableRoles = _roleManager.Roles.Select(r => r.Name).ToList();

            var model = new UserManagementViewModel
            {
                Id = user.Id,
                NomPrenom = user.NomPrenom,
                Email = user.Email,
                Telephone = user.Telephone,
                Roles = roles.ToList(),
                AvailableRoles = availableRoles,
                IsActive = user.IsActive
            };

            return View(model);
        }

        // Edit User POST - FIXED VERSION WITH PROPER REDIRECTION
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUserPost(UserManagementViewModel model)
        {
            try
            {
                Console.WriteLine($"EditUserPost called with: Id={model.Id}, Name={model.NomPrenom}, Email={model.Email}, Phone={model.Telephone}");
                Console.WriteLine($"Roles: {(model.Roles != null ? string.Join(", ", model.Roles) : "null")}");

                // Remove password validation for edit operations
                ModelState.Remove("Password");

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    Console.WriteLine($"ModelState errors: {string.Join(", ", errors)}");

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = string.Join(", ", errors) });
                    }

                    // Reload available roles for non-AJAX requests
                    model.AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList();
                    return View("EditUser", model);
                }

                // Enhanced validation
                if (!IsValidPhoneNumber(model.Telephone))
                {
                    Console.WriteLine("Phone validation failed");
                    var message = "Phone number must be exactly 8 digits";

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = message });
                    }

                    ModelState.AddModelError("Telephone", message);
                    model.AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList();
                    return View("EditUser", model);
                }

                if (!IsValidEmail(model.Email))
                {
                    Console.WriteLine("Email validation failed");
                    var message = "Please enter a valid email address";

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = message });
                    }

                    ModelState.AddModelError("Email", message);
                    model.AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList();
                    return View("EditUser", model);
                }

                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    Console.WriteLine("User not found");
                    var message = "User not found";

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = message });
                    }

                    return NotFound();
                }

                // Check if email is being changed and if it already exists
                if (user.Email != model.Email)
                {
                    var existingUser = await _userManager.FindByEmailAsync(model.Email);
                    if (existingUser != null && existingUser.Id != model.Id)
                    {
                        Console.WriteLine("Email already exists");
                        var message = "A user with this email already exists";

                        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        {
                            return Json(new { success = false, message = message });
                        }

                        ModelState.AddModelError("Email", message);
                        model.AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList();
                        return View("EditUser", model);
                    }
                }

                // Update user properties
                user.NomPrenom = model.NomPrenom;
                user.Email = model.Email;
                user.UserName = model.Email;
                user.Telephone = model.Telephone;

                Console.WriteLine($"Updating user: {user.NomPrenom}, {user.Email}, {user.Telephone}");

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    var errors = updateResult.Errors.Select(e => e.Description);
                    Console.WriteLine($"Update failed: {string.Join(", ", errors)}");
                    var errorMessage = string.Join(", ", errors);

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = errorMessage });
                    }

                    ModelState.AddModelError("", errorMessage);
                    model.AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList();
                    return View("EditUser", model);
                }

                // Update user roles
                var currentRoles = await _userManager.GetRolesAsync(user);
                Console.WriteLine($"Current roles: {string.Join(", ", currentRoles)}");

                if (currentRoles.Any())
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    if (!removeResult.Succeeded)
                    {
                        Console.WriteLine("Failed to remove current roles");
                    }
                }

                if (model.Roles != null && model.Roles.Any())
                {
                    Console.WriteLine($"Adding roles: {string.Join(", ", model.Roles)}");
                    var roleResult = await _userManager.AddToRolesAsync(user, model.Roles);
                    if (!roleResult.Succeeded)
                    {
                        var errors = roleResult.Errors.Select(e => e.Description);
                        Console.WriteLine($"Role assignment failed: {string.Join(", ", errors)}");
                        var errorMessage = "User updated but failed to assign roles: " + string.Join(", ", errors);

                        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        {
                            return Json(new { success = false, message = errorMessage });
                        }

                        ModelState.AddModelError("", errorMessage);
                        model.AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList();
                        return View("EditUser", model);
                    }
                }

                Console.WriteLine("User updated successfully");
                TempData["Success"] = $"✅ User '{model.NomPrenom}' updated successfully!";

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        success = true,
                        message = "User updated successfully",
                        redirect = Url.Action("UsersList", "Admin")
                    });
                }

                return RedirectToAction("UsersList");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                var message = "An unexpected error occurred while updating the user";

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = message });
                }

                ModelState.AddModelError("", message);
                model.AvailableRoles = _roleManager.Roles.Select(r => r.Name).ToList();
                return View("EditUser", model);
            }
        }

        // Deactivate User (Soft Delete)
        [HttpPost]
        public async Task<IActionResult> DeactivateUser(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return Json(new { success = false, message = "User ID is required" });
                }

                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                // Check if trying to deactivate the current user
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser?.Id == id)
                {
                    return Json(new { success = false, message = "You cannot deactivate your own account" });
                }

                // Soft delete - just deactivate
                user.IsActive = false;
                user.LockoutEnd = DateTimeOffset.MaxValue;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    return Json(new { success = true, message = "User deactivated successfully" });
                }

                var errors = result.Errors.Select(e => e.Description);
                return Json(new { success = false, message = string.Join(", ", errors) });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deactivating user: {ex.Message}");
                return Json(new { success = false, message = "An unexpected error occurred" });
            }
        }

        // Reactivate User
        [HttpPost]
        public async Task<IActionResult> ReactivateUser(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                user.IsActive = true;
                user.LockoutEnd = null;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    return Json(new { success = true, message = "User reactivated successfully" });
                }

                var errors = result.Errors.Select(e => e.Description);
                return Json(new { success = false, message = string.Join(", ", errors) });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reactivating user: {ex.Message}");
                return Json(new { success = false, message = "An unexpected error occurred" });
            }
        }

        // Password Management
        [HttpPost]
        public async Task<IActionResult> ResetUserPassword(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                var newPassword = GenerateSecurePassword();
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

                if (result.Succeeded)
                {
                    return Json(new { success = true, newPassword = newPassword });
                }

                return Json(new { success = false, message = "Failed to reset password" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resetting password: {ex.Message}");
                return Json(new { success = false, message = "An unexpected error occurred" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SetUserPassword(string userId, string newPassword)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

                if (result.Succeeded)
                {
                    return Json(new { success = true, message = "Password updated successfully" });
                }

                return Json(new { success = false, message = "Failed to update password" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting password: {ex.Message}");
                return Json(new { success = false, message = "An unexpected error occurred" });
            }
        }

        // Account Settings
        [HttpGet]
        public async Task<IActionResult> AccountSettings()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }

            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var roles = await _userManager.GetRolesAsync(user);

            var model = new UserManagementViewModel
            {
                NomPrenom = user.NomPrenom,
                Email = user.Email,
                Telephone = user.Telephone,
                Role = roles.FirstOrDefault() ?? "User"
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

        // Categories Management
        public async Task<IActionResult> Categories()
        {
            var categories = await _context.Categories.ToListAsync();
            return View(categories);
        }

        public IActionResult AddCategorie()
        {
            return View(new CategorieManagementViewModel());
        }

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
                    };
                    _context.Add(categorie);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "✅ Category created successfully";

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true });
                    }
                    return RedirectToAction(nameof(Categories));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("SQL ERROR: " + ex.Message);
                    ModelState.AddModelError(string.Empty, "An error occurred while saving. Try again.");
                }
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView(model);
            }
            return View(model);
        }

        public async Task<IActionResult> EditCategorie(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var categorie = await _context.Categories.FindAsync(id);
            if (categorie == null)
            {
                return NotFound();
            }

            var model = new CategorieManagementViewModel
            {
                Id = categorie.Id,
                Description = categorie.Description
            };

            return View(model);
        }

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

                    categorie.Description = model.Description;

                    _context.Update(categorie);
                    await _context.SaveChangesAsync();

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true });
                    }
                    return RedirectToAction(nameof(Categories));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("SQL ERROR: " + ex.Message);
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the category. Please try again.");
                }
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView(model);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategorie(int id)
        {
            var categorie = await _context.Categories.FindAsync(id);
            if (categorie == null)
            {
                return NotFound();
            }

            _context.Categories.Remove(categorie);
            await _context.SaveChangesAsync();

            return RedirectToAction("Categories");
        }

        // Articles Management
        public async Task<IActionResult> Articles(int categoryId)
        {
            var articles = await _context.Articles
                .Where(a => a.CategorieId == categoryId)
                .ToListAsync();
            ViewBag.CategoryId = categoryId;
            return View(articles);
        }

        public IActionResult AddArticle(int categoryId)
        {
            ViewBag.CategoryId = categoryId;
            return PartialView(new ArticleViewModel { CategorieId = categoryId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddArticle(ArticleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    System.Diagnostics.Debug.WriteLine($"Validation Error: {error.ErrorMessage}");
                }
                return PartialView(model);
            }

            try
            {
                if (await _context.Articles.AnyAsync(a => a.IdArticle == model.IdArticle))
                {
                    ModelState.AddModelError("IdArticle", "An article with this ID already exists.");
                    return PartialView(model);
                }

                var article = new Article
                {
                    IdArticle = model.IdArticle,
                    Description = model.Description,
                    CategorieId = model.CategorieId,
                    PrixHistorique = new List<Prix>()
                };
                _context.Articles.Add(article);
                await _context.SaveChangesAsync();
                TempData["Success"] = "✅ Article created successfully";
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database Error: {ex.Message}");
                ModelState.AddModelError("", $"Error saving article: {ex.Message}");
                return PartialView(model);
            }
        }

        public async Task<IActionResult> EditArticle(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            var model = new ArticleViewModel
            {
                IdArticle = article.IdArticle,
                Description = article.Description,
                CategorieId = article.CategorieId
            };
            return PartialView(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditArticle(int id, ArticleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    System.Diagnostics.Debug.WriteLine($"Validation Error: {error.ErrorMessage}");
                }
                return PartialView(model);
            }

            try
            {
                var article = await _context.Articles.FindAsync(id);
                if (article == null)
                {
                    return NotFound();
                }

                if (await _context.Articles.AnyAsync(a => a.IdArticle == model.IdArticle && a.Id != id))
                {
                    ModelState.AddModelError("IdArticle", "An article with this ID already exists.");
                    return PartialView(model);
                }

                article.IdArticle = model.IdArticle;
                article.Description = model.Description;
                article.CategorieId = model.CategorieId;

                _context.Update(article);
                await _context.SaveChangesAsync();
                TempData["Success"] = "✅ Article updated successfully";
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database Error: {ex.Message}");
                ModelState.AddModelError("", $"Error updating article: {ex.Message}");
                return PartialView(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteArticle(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();
            TempData["Success"] = "✅ Article deleted successfully";
            return RedirectToAction("Articles", new { categoryId = article.CategorieId });
        }

        // Price Management
        public async Task<IActionResult> Prices(int? categoryId)
        {
            var query = _context.Articles.AsQueryable();
            if (categoryId.HasValue)
            {
                query = query.Where(a => a.CategorieId == categoryId.Value);
            }

            var articles = await query.ToListAsync();
            ViewBag.CategoryId = categoryId;
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(articles);
        }

        [HttpGet]
        [Route("Admin/ManagePrices/{id:int}")]
        public async Task<IActionResult> ManagePrices(int id)
        {
            var article = await _context.Articles
                .Include(a => a.PrixHistorique)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (article == null)
                return NotFound();

            var model = new PrixViewModel
            {
                ArticleId = article.Id,
                DateDebut = DateTime.Today
            };

            ViewBag.Article = article;
            ViewBag.PriceHistory = article.PrixHistorique.OrderByDescending(p => p.DateDebut);
            ViewBag.CategoryId = Request.Query["categoryId"].ToString();
            return PartialView(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPrice(PrixViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var article = await _context.Articles.FindAsync(model.ArticleId);
                ViewBag.Article = article;
                ViewBag.PriceHistory = await _context.Prix
                    .Where(p => p.ArticleId == model.ArticleId && p.DateFin == null)
                    .OrderByDescending(p => p.DateDebut)
                    .ToListAsync();
                ViewBag.CategoryId = Request.Query["categoryId"].ToString();
                return PartialView("ManagePrices", model);
            }

            try
            {
                var article = await _context.Articles.FindAsync(model.ArticleId);
                if (article == null)
                    return NotFound();

                var activePrice = await _context.Prix
                    .Where(p => p.ArticleId == model.ArticleId && p.DateFin == null)
                    .FirstOrDefaultAsync();
                if (activePrice != null)
                {
                    activePrice.DateFin = model.DateDebut.AddDays(-1);
                    _context.Update(activePrice);
                }

                var prix = new Prix
                {
                    ArticleId = model.ArticleId,
                    Valeur = model.Valeur,
                    DateDebut = model.DateDebut,
                    DateFin = model.DateFin
                };
                _context.Prix.Add(prix);

                article.PrixUnitaireHT = model.Valeur;
                _context.Update(article);

                await _context.SaveChangesAsync();
                TempData["Success"] = "✅ Prix ajouté avec succès";
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur base de données: {ex.Message}");
                ModelState.AddModelError("", $"Erreur lors de l'ajout du prix: {ex.Message}");
                var article = await _context.Articles.FindAsync(model.ArticleId);
                ViewBag.Article = article;
                ViewBag.PriceHistory = await _context.Prix
                    .Where(p => p.ArticleId == model.ArticleId)
                    .OrderByDescending(p => p.DateDebut)
                    .ToListAsync();
                ViewBag.CategoryId = Request.Query["categoryId"].ToString();
                return PartialView("ManagePrices", model);
            }
        }

        [HttpGet]
        [Route("Admin/EditPrice/{id:int}")]
        public async Task<IActionResult> EditPrice(int id)
        {
            var prix = await _context.Prix.FindAsync(id);
            if (prix == null)
                return NotFound();

            var article = await _context.Articles.FindAsync(prix.ArticleId);
            if (article == null)
                return NotFound();

            var model = new PrixViewModel
            {
                Id = prix.Id,
                ArticleId = prix.ArticleId,
                Valeur = prix.Valeur,
                DateDebut = prix.DateDebut,
                DateFin = prix.DateFin
            };

            ViewBag.Article = article;
            ViewBag.CategoryId = Request.Query["categoryId"].ToString();
            return PartialView(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPrice(PrixViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var article = await _context.Articles.FindAsync(model.ArticleId);
                ViewBag.Article = article;
                ViewBag.CategoryId = Request.Query["categoryId"].ToString();
                return PartialView(model);
            }

            try
            {
                var prix = await _context.Prix.FindAsync(model.Id);
                if (prix == null)
                    return NotFound();

                var article = await _context.Articles.FindAsync(model.ArticleId);
                if (article == null)
                    return NotFound();

                prix.Valeur = model.Valeur;
                prix.DateDebut = model.DateDebut;
                prix.DateFin = model.DateFin;
                _context.Update(prix);

                if (prix.DateFin == null)
                {
                    article.PrixUnitaireHT = model.Valeur;
                }
                else
                {
                    var activePrice = await _context.Prix
                        .Where(p => p.ArticleId == model.ArticleId && p.DateFin == null)
                        .FirstOrDefaultAsync();
                    if (activePrice != null)
                    {
                        article.PrixUnitaireHT = activePrice.Valeur;
                    }
                    else
                    {
                        var mostRecentPrice = await _context.Prix
                            .Where(p => p.ArticleId == model.ArticleId)
                            .OrderByDescending(p => p.DateDebut)
                            .FirstOrDefaultAsync();
                        article.PrixUnitaireHT = mostRecentPrice?.Valeur ?? 0m;
                    }
                }
                _context.Update(article);

                await _context.SaveChangesAsync();
                TempData["Success"] = "✅ Prix mis à jour avec succès";
                return Json(new
                {
                    success = true,
                    id = prix.Id,
                    valeur = prix.Valeur.ToString("C"),
                    dateDebut = prix.DateDebut.ToString("yyyy-MM-dd"),
                    dateFin = prix.DateFin?.ToString("yyyy-MM-dd") ?? "Actif"
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur base de données: {ex.Message}");
                ModelState.AddModelError("", $"Erreur lors de la mise à jour du prix: {ex.Message}");
                var article = await _context.Articles.FindAsync(model.ArticleId);
                ViewBag.Article = article;
                ViewBag.CategoryId = Request.Query["categoryId"].ToString();
                return PartialView(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/DeletePrice/{id:int}")]
        public async Task<IActionResult> DeletePrice(int id)
        {
            try
            {
                var prix = await _context.Prix.FindAsync(id);
                if (prix == null)
                    return Json(new { success = false, message = "Prix non trouvé." });

                var article = await _context.Articles.FindAsync(prix.ArticleId);
                if (article == null)
                    return Json(new { success = false, message = "Article non trouvé." });

                _context.Prix.Remove(prix);

                var activePrice = await _context.Prix
                    .Where(p => p.ArticleId == prix.ArticleId && p.DateFin == null)
                    .FirstOrDefaultAsync();
                article.PrixUnitaireHT = activePrice?.Valeur ?? 0m;
                _context.Update(article);

                await _context.SaveChangesAsync();
                TempData["Success"] = "✅ Prix supprimé avec succès";
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur base de données: {ex.Message}");
                return Json(new { success = false, message = $"Erreur lors de la suppression du prix: {ex.Message}" });
            }
        }

        // Helper Methods
        private async Task<Dictionary<string, int>> GetUsersByRoleAsync()
        {
            var roleStats = new Dictionary<string, int>();
            var roles = await _roleManager.Roles.ToListAsync();

            foreach (var role in roles)
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
                roleStats[role.Name] = usersInRole.Count;
            }

            return roleStats;
        }

        private async Task<List<UserManagementViewModel>> GetRecentUsersAsync()
        {
            var recentUsers = await _userManager.Users
                .OrderByDescending(u => u.Id)
                .Take(5)
                .ToListAsync();

            var userList = new List<UserManagementViewModel>();
            foreach (var user in recentUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(new UserManagementViewModel
                {
                    Id = user.Id,
                    NomPrenom = user.NomPrenom,
                    Email = user.Email,
                    Telephone = user.Telephone,
                    Roles = roles.ToList(),
                    IsActive = user.IsActive
                });
            }

            return userList;
        }

        private bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
                return false;

            var digitsOnly = new string(phoneNumber.Where(char.IsDigit).ToArray());
            return digitsOnly.Length == 8;
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private string GenerateSecurePassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            var random = new Random();
            var password = new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            return password + "A1!";
        }


        // Dashboard with statistics
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var totalUsers = await _userManager.Users.CountAsync();
                var activeUsers = await _userManager.Users.Where(u => u.IsActive).CountAsync();
                var inactiveUsers = await _userManager.Users.Where(u => !u.IsActive).CountAsync();

                Console.WriteLine($"Dashboard Stats - Total: {totalUsers}, Active: {activeUsers}, Inactive: {inactiveUsers}");

                var dashboardData = new DashboardViewModel
                {
                    TotalUsers = totalUsers,
                    ActiveUsers = activeUsers,
                    InactiveUsers = inactiveUsers,
                    UsersByRole = await GetUsersByRoleAsync(),
                    RecentUsers = await GetRecentUsersAsync(),

                    // Prepare chart data
                    UserStatusData = new List<ChartDataPoint>
                    {
                        new ChartDataPoint { Label = "Active Users", Value = activeUsers, Color = "#28a745" },
                        new ChartDataPoint { Label = "Inactive Users", Value = inactiveUsers, Color = "#dc3545" }
                    },

                    RoleDistributionData = (await GetUsersByRoleAsync()).Select(kvp => new ChartDataPoint
                    {
                        Label = kvp.Key,
                        Value = kvp.Value,
                        Color = GetRoleColor(kvp.Key)
                    }).ToList()
                };

                return View(dashboardData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Dashboard error: {ex.Message}");
                // Return empty dashboard on error
                return View(new DashboardViewModel());
            }
        }
        // Enhanced method with ALL relationship checks including ExtractedPdfData
        private async Task<UserRelationshipInfo> GetUserRelationshipDetails(string userId)
        {
            try
            {
                Console.WriteLine($"=== CHECKING RELATIONSHIPS FOR USER ID: {userId} ===");

                var relationshipInfo = new UserRelationshipInfo
                {
                    UserId = userId,
                    HasRelatedData = false,
                    RelationshipDetails = new List<string>()
                };

                // Check Redevance calculations
                Console.WriteLine("Checking Redevance records...");
                var redevanceCount = await _context.Set<Redevance>()
                    .CountAsync(r => r.CalculeParId == userId);
                Console.WriteLine($"Redevance count for user {userId}: {redevanceCount}");
                if (redevanceCount > 0)
                {
                    relationshipInfo.HasRelatedData = true;
                    relationshipInfo.RelationshipDetails.Add($"{redevanceCount} calcul(s) de redevance");
                    Console.WriteLine($"✓ Found {redevanceCount} Redevance records");
                }

                // Check created Motifs
                Console.WriteLine("Checking created Motifs...");
                var createdMotifsCount = await _context.Set<Motif>()
                    .CountAsync(m => m.CreatedBy == userId);
                Console.WriteLine($"Created Motifs count for user {userId}: {createdMotifsCount}");
                if (createdMotifsCount > 0)
                {
                    relationshipInfo.HasRelatedData = true;
                    relationshipInfo.RelationshipDetails.Add($"{createdMotifsCount} motif(s) créé(s)");
                    Console.WriteLine($"✓ Found {createdMotifsCount} created Motif records");
                }

                // Check read Motifs
                Console.WriteLine("Checking read Motifs...");
                var readMotifsCount = await _context.Set<Motif>()
                    .CountAsync(m => m.ReadBy == userId);
                Console.WriteLine($"Read Motifs count for user {userId}: {readMotifsCount}");
                if (readMotifsCount > 0)
                {
                    relationshipInfo.HasRelatedData = true;
                    relationshipInfo.RelationshipDetails.Add($"{readMotifsCount} motif(s) lu(s)");
                    Console.WriteLine($"✓ Found {readMotifsCount} read Motif records");
                }

                // Check PNC Commission calculations
                Console.WriteLine("Checking PNC Commission calculations...");
                var pncCommissionCount = await _context.Set<PncCommissionCalculation>()
                    .CountAsync(p => p.CalculatedBy == userId);
                Console.WriteLine($"PNC Commission count for user {userId}: {pncCommissionCount}");
                if (pncCommissionCount > 0)
                {
                    relationshipInfo.HasRelatedData = true;
                    relationshipInfo.RelationshipDetails.Add($"{pncCommissionCount} calcul(s) de commission PNC");
                    Console.WriteLine($"✓ Found {pncCommissionCount} PNC Commission records");
                }

                // Check EtatDesVentesArrivee entries
                Console.WriteLine("Checking EtatDesVentesArrivee entries...");
                var etatDesVentesCount = await _context.Set<EtatDesVentesArrivee>()
                    .CountAsync(e => e.AgentSaisieId == userId);
                Console.WriteLine($"EtatDesVentesArrivee count for user {userId}: {etatDesVentesCount}");
                if (etatDesVentesCount > 0)
                {
                    relationshipInfo.HasRelatedData = true;
                    relationshipInfo.RelationshipDetails.Add($"{etatDesVentesCount} état(s) de ventes saisi(s)");
                    Console.WriteLine($"✓ Found {etatDesVentesCount} EtatDesVentesArrivee records");
                }

                // NEW: Check ExtractedPdfData entries
                Console.WriteLine("Checking ExtractedPdfData entries...");
                var extractedPdfCount = await _context.Set<ExtractedPdfData>()
                    .CountAsync(e => e.ExtractedBy == userId);
                Console.WriteLine($"ExtractedPdfData count for user {userId}: {extractedPdfCount}");
                if (extractedPdfCount > 0)
                {
                    relationshipInfo.HasRelatedData = true;
                    relationshipInfo.RelationshipDetails.Add($"{extractedPdfCount} extraction(s) PDF effectuée(s)");
                    Console.WriteLine($"✓ Found {extractedPdfCount} ExtractedPdfData records");
                }

                Console.WriteLine($"=== FINAL RESULT FOR USER {userId} ===");
                Console.WriteLine($"HasRelatedData: {relationshipInfo.HasRelatedData}");
                Console.WriteLine($"Total relationships: {relationshipInfo.TotalRelationships}");
                Console.WriteLine($"Details: {string.Join(" | ", relationshipInfo.RelationshipDetails)}");
                Console.WriteLine("=== END RELATIONSHIP CHECK ===");

                return relationshipInfo;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR in GetUserRelationshipDetails: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return new UserRelationshipInfo
                {
                    UserId = userId,
                    HasRelatedData = true, // Err on the side of caution
                    RelationshipDetails = new List<string> { "Erreur lors de la vérification des relations" }
                };
            }
        }

        // COMPLETELY REWRITTEN DeleteOrDeactivateUser method with better logic
        [HttpPost]
        public async Task<IActionResult> DeleteOrDeactivateUser(string id)
        {
            try
            {
                Console.WriteLine($"🔥 DELETE/DEACTIVATE REQUEST for user ID: {id}");

                if (string.IsNullOrEmpty(id))
                {
                    Console.WriteLine("❌ User ID is null or empty");
                    return Json(new { success = false, message = "User ID is required" });
                }

                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    Console.WriteLine($"❌ User not found with ID: {id}");
                    return Json(new { success = false, message = "User not found" });
                }

                Console.WriteLine($"✓ Found user: {user.NomPrenom} ({user.Email}) - ID: {user.Id}");

                // Check if trying to delete/deactivate the current user
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser?.Id == id)
                {
                    Console.WriteLine("❌ Attempting to delete current user - BLOCKED");
                    return Json(new { success = false, message = "You cannot delete or deactivate your own account" });
                }

                // Get user roles for additional checks
                var userRoles = await _userManager.GetRolesAsync(user);
                Console.WriteLine($"User roles: {string.Join(", ", userRoles)}");

                // Check if user has related data
                Console.WriteLine("🔍 Starting relationship check...");
                var relationshipInfo = await GetUserRelationshipDetails(id);

                Console.WriteLine($"🔍 Relationship check completed:");
                Console.WriteLine($"   - HasRelatedData: {relationshipInfo.HasRelatedData}");
                Console.WriteLine($"   - Total relationships: {relationshipInfo.TotalRelationships}");
                Console.WriteLine($"   - Details: {string.Join(" | ", relationshipInfo.RelationshipDetails)}");

                if (relationshipInfo.HasRelatedData)
                {
                    Console.WriteLine("⚠️ User has related data - DEACTIVATING instead of deleting");

                    // User has related data - deactivate instead of delete
                    user.IsActive = false;
                    user.LockoutEnd = DateTimeOffset.MaxValue;

                    var updateResult = await _userManager.UpdateAsync(user);

                    if (updateResult.Succeeded)
                    {
                        var detailsMessage = string.Join(", ", relationshipInfo.RelationshipDetails);
                        Console.WriteLine($"✅ User DEACTIVATED successfully. Details: {detailsMessage}");

                        return Json(new
                        {
                            success = true,
                            message = $"Impossible de supprimer un utilisateur lié à des actions en cours ({detailsMessage}). L'utilisateur a été désactivé.",
                            action = "deactivated",
                            details = relationshipInfo.RelationshipDetails,
                            userName = user.NomPrenom
                        });
                    }
                    else
                    {
                        var errors = updateResult.Errors.Select(e => e.Description);
                        Console.WriteLine($"❌ Failed to deactivate user: {string.Join(", ", errors)}");
                        return Json(new { success = false, message = string.Join(", ", errors) });
                    }
                }
                else
                {
                    Console.WriteLine("✅ User has NO related data - proceeding with DELETION");

                    // User has no related data - safe to delete
                    var deleteResult = await _userManager.DeleteAsync(user);

                    if (deleteResult.Succeeded)
                    {
                        Console.WriteLine($"✅ User DELETED successfully: {user.NomPrenom}");
                        return Json(new
                        {
                            success = true,
                            message = $"L'utilisateur {user.NomPrenom} a été supprimé définitivement - aucune donnée liée trouvée",
                            action = "deleted",
                            userName = user.NomPrenom
                        });
                    }
                    else
                    {
                        var errors = deleteResult.Errors.Select(e => e.Description);
                        Console.WriteLine($"❌ Failed to delete user: {string.Join(", ", errors)}");
                        return Json(new { success = false, message = string.Join(", ", errors) });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 EXCEPTION in DeleteOrDeactivateUser: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new { success = false, message = "Une erreur inattendue s'est produite" });
            }
        }

        // Enhanced test method to verify ALL relationships
        [HttpGet]
        public async Task<IActionResult> TestUserRelationships(string id)
        {
            try
            {
                Console.WriteLine($"🧪 TESTING relationships for user ID: {id}");

                if (string.IsNullOrEmpty(id))
                {
                    return Json(new { success = false, message = "User ID is required" });
                }

                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                // Test each relationship individually
                var testResults = new Dictionary<string, object>();

                // Test Redevance
                var redevanceCount = await _context.Set<Redevance>().CountAsync(r => r.CalculeParId == id);
                testResults["Redevance"] = new { count = redevanceCount, hasData = redevanceCount > 0 };

                // Test Motif (Created)
                var createdMotifsCount = await _context.Set<Motif>().CountAsync(m => m.CreatedBy == id);
                testResults["MotifsCreated"] = new { count = createdMotifsCount, hasData = createdMotifsCount > 0 };

                // Test Motif (Read)
                var readMotifsCount = await _context.Set<Motif>().CountAsync(m => m.ReadBy == id);
                testResults["MotifsRead"] = new { count = readMotifsCount, hasData = readMotifsCount > 0 };

                // Test PNC Commission
                var pncCount = await _context.Set<PncCommissionCalculation>().CountAsync(p => p.CalculatedBy == id);
                testResults["PncCommission"] = new { count = pncCount, hasData = pncCount > 0 };

                // Test EtatDesVentesArrivee
                var etatCount = await _context.Set<EtatDesVentesArrivee>().CountAsync(e => e.AgentSaisieId == id);
                testResults["EtatDesVentes"] = new { count = etatCount, hasData = etatCount > 0 };

                // Test ExtractedPdfData
                var pdfCount = await _context.Set<ExtractedPdfData>().CountAsync(e => e.ExtractedBy == id);
                testResults["ExtractedPdf"] = new { count = pdfCount, hasData = pdfCount > 0 };

                // Get full relationship info
                var relationshipInfo = await GetUserRelationshipDetails(id);

                return Json(new
                {
                    success = true,
                    userId = id,
                    userName = user.NomPrenom,
                    userEmail = user.Email,
                    hasRelatedData = relationshipInfo.HasRelatedData,
                    details = relationshipInfo.RelationshipDetails,
                    totalRelationships = relationshipInfo.TotalRelationships,
                    individualTests = testResults
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 EXCEPTION in TestUserRelationships: {ex.Message}");
                return Json(new
                {
                    success = false,
                    message = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        // Method to get detailed motif information for a user (for debugging)
        [HttpGet]
        public async Task<IActionResult> GetUserMotifs(string id)
        {
            try
            {
                var createdMotifs = await _context.Set<Motif>()
                    .Where(m => m.CreatedBy == id)
                    .Select(m => new {
                        m.Id,
                        m.NumeroEtatVenteArrivee,
                        m.Description,
                        m.DateCreation,
                        m.Status
                    })
                    .ToListAsync();

                var readMotifs = await _context.Set<Motif>()
                    .Where(m => m.ReadBy == id)
                    .Select(m => new {
                        m.Id,
                        m.NumeroEtatVenteArrivee,
                        m.Description,
                        m.DateLecture,
                        m.Status
                    })
                    .ToListAsync();

                return Json(new
                {
                    success = true,
                    userId = id,
                    createdMotifs = createdMotifs,
                    readMotifs = readMotifs,
                    createdCount = createdMotifs.Count,
                    readCount = readMotifs.Count
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
        private string GetRoleColor(string roleName)
        {
            return roleName switch
            {
                "Admin" => "#007bff",
                "AgentSaisie" => "#28a745",
                "DGF" => "#ffc107",
                "ResponsableCatering" => "#17a2b8",
                _ => "#6c757d"
            };
        }
    }
}