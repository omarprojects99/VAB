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
        // GET: Add Category
        public IActionResult AddCategorie()
        {
            return View(new CategorieManagementViewModel());
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

        // GET: Edit Category
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

        // POST: Edit Category
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


        // Other actions (e.g., Categories, DeleteCategorie) assumed unchanged

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

            return RedirectToAction("Categories"); // Adjust this to your listing view
        }
        // GET: /Admin/Articles?categoryId={id}
        public async Task<IActionResult> Articles(int categoryId)
        {
            var articles = await _context.Articles
                .Where(a => a.CategorieId == categoryId)
                .ToListAsync();
            ViewBag.CategoryId = categoryId;
            return View(articles);
        }

        // GET: /Admin/AddArticle?categoryId={id}
        // GET: /Admin/AddArticle?categoryId={id}
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
                    QuantiteDotation = 0,
                    QuantiteComplementaire = 0,
                    TotalEmbarque = 0,
                    QuantiteCasse = 0,
                    QuantiteOffert = 0,
                    QuantiteVendu = 0,
                    PrixUnitaireHT = 0m,
                    Valeur = 0m,
                    Restant = 0,
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
        // POST: /Admin/EditArticle
        // GET: /Admin/EditArticle/{id}
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

        // POST: /Admin/EditArticle
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

                // Check for duplicate IdArticle, excluding the current article
                if (await _context.Articles.AnyAsync(a => a.IdArticle == model.IdArticle && a.Id != id))
                {
                    ModelState.AddModelError("IdArticle", "An article with this ID already exists.");
                    return PartialView(model);
                }

                article.IdArticle = model.IdArticle;
                article.Description = model.Description;
                article.CategorieId = model.CategorieId;
                // Other properties remain unchanged

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

        // POST: /Admin/DeleteArticle
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
        // Price Management Actions
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
                    .Where(p => p.ArticleId == model.ArticleId)
                    .OrderByDescending(p => p.DateDebut)
                    .ToListAsync();
                return PartialView("ManagePrices", model);
            }

            try
            {
                var article = await _context.Articles.FindAsync(model.ArticleId);
                if (article == null)
                    return NotFound();

                // Set DateFin for previous active price (if any)
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

                // Update Article's current price
                article.PrixUnitaireHT = model.Valeur;
                _context.Update(article);

                await _context.SaveChangesAsync();
                TempData["Success"] = "✅ Price added successfully";
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database Error: {ex.Message}");
                ModelState.AddModelError("", $"Error adding price: {ex.Message}");
                var article = await _context.Articles.FindAsync(model.ArticleId);
                ViewBag.Article = article;
                ViewBag.PriceHistory = await _context.Prix
                    .Where(p => p.ArticleId == model.ArticleId)
                    .OrderByDescending(p => p.DateDebut)
                    .ToListAsync();
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

                // Update Article's current price if this is the active price
                if (prix.DateFin == null)
                {
                    article.PrixUnitaireHT = model.Valeur;
                    _context.Update(article);
                }
                else
                {
                    // If the edited price is no longer active, set PrixUnitaireHT to the current active price
                    var activePrice = await _context.Prix
                        .Where(p => p.ArticleId == model.ArticleId && p.DateFin == null)
                        .FirstOrDefaultAsync();
                    article.PrixUnitaireHT = activePrice?.Valeur ?? 0m;
                    _context.Update(article);
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "✅ Price updated successfully";
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database Error: {ex.Message}");
                ModelState.AddModelError("", $"Error updating price: {ex.Message}");
                var article = await _context.Articles.FindAsync(model.ArticleId);
                ViewBag.Article = article;
                return PartialView(model);
            }
        }
    }





}







