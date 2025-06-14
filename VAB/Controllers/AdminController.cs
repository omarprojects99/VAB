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

        //// Dashboard with statistics
        //public async Task<IActionResult> Dashboard()
        //{
        //    var dashboardData = new DashboardViewModel
        //    {
        //        TotalUsers = await _userManager.Users.CountAsync(),
        //        ActiveUsers = await _userManager.Users.Where(u => u.IsActive).CountAsync(),
        //        InactiveUsers = await _userManager.Users.Where(u => !u.IsActive).CountAsync(),
        //        UsersByRole = await GetUsersByRoleAsync(),
        //        RecentUsers = await GetRecentUsersAsync()
        //    };

        //    return View(dashboardData);
        //}

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
        // Add User POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUser(UserManagementViewModel model)
        {
            try
            {
                Console.WriteLine($"AddUser called with: {model.NomPrenom}, {model.Email}, {model.Telephone}, {model.Role}");

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
                    return Json(new { success = false, message = "Phone number must be exactly 8 digits" });
                }

                if (!IsValidEmail(model.Email))
                {
                    Console.WriteLine("Email validation failed");
                    return Json(new { success = false, message = "Please enter a valid email address" });
                }

                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    Console.WriteLine("User already exists");
                    return Json(new { success = false, message = "A user with this email already exists" });
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
                        return Json(new { success = false, message = "Selected role does not exist" });
                    }

                    // Assign Role
                    var roleResult = await _userManager.AddToRoleAsync(user, model.Role);
                    if (!roleResult.Succeeded)
                    {
                        Console.WriteLine("Failed to assign role");
                        await _userManager.DeleteAsync(user); // Cleanup
                        return Json(new { success = false, message = "Failed to assign role to user" });
                    }

                    // Set TempData for success message like other forms
                    TempData["Success"] = $"✅ User '{model.NomPrenom}' created successfully! Default password: {defaultPassword}";

                    Console.WriteLine("User creation completed successfully");
                    return Json(new
                    {
                        success = true,
                        message = "User created successfully",
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
                return Json(new { success = false, message = "An unexpected error occurred while creating the user" });
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

        // Edit User POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUserPost(UserManagementViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                // Enhanced validation
                if (!IsValidPhoneNumber(model.Telephone))
                {
                    return Json(new { success = false, message = "Phone number must be exactly 8 digits" });
                }

                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                // Check if email is being changed and if it already exists
                if (user.Email != model.Email)
                {
                    var existingUser = await _userManager.FindByEmailAsync(model.Email);
                    if (existingUser != null && existingUser.Id != model.Id)
                    {
                        return Json(new { success = false, message = "A user with this email already exists" });
                    }
                }

                user.NomPrenom = model.NomPrenom;
                user.Email = model.Email;
                user.UserName = model.Email;
                user.Telephone = model.Telephone;

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    var errors = updateResult.Errors.Select(e => e.Description);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                // Update user roles
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

                if (model.Roles != null && model.Roles.Any())
                {
                    var roleResult = await _userManager.AddToRolesAsync(user, model.Roles);
                    if (!roleResult.Succeeded)
                    {
                        var errors = roleResult.Errors.Select(e => e.Description);
                        return Json(new { success = false, message = "User updated but failed to assign roles: " + string.Join(", ", errors) });
                    }
                }

                return Json(new { success = true, message = "User updated successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user: {ex.Message}");
                return Json(new { success = false, message = "An unexpected error occurred while updating the user" });
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