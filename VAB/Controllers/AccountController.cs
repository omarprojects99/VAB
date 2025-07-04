using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VAB.Models;
using VAB.ViewModel;
using System.ComponentModel.DataAnnotations;

namespace VAB.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
            {
                // Check for empty fields
                if (string.IsNullOrWhiteSpace(model.Email))
                {
                    ModelState.AddModelError("Email", "Email est requis");
                }

                if (string.IsNullOrWhiteSpace(model.Password))
                {
                    ModelState.AddModelError("Password", " Le mot de passe est requis");
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Validate email format
                if (!IsValidEmail(model.Email))
                {
                    ModelState.AddModelError("Email", "Veuillez entrer une adresse e-mail valide");
                    return View(model);
                }

                // Find user by email
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user == null)
                {
                    _logger.LogWarning("Login attempt with non-existent email: {Email}", model.Email);
                    ModelState.AddModelError("", "Invalid email or password");
                    return View(model);
                }

                // Check if user account is active
                if (!user.IsActive)
                {
                    _logger.LogWarning("Login attempt with inactive account: {Email}", model.Email);
                    ModelState.AddModelError("", "Your account has been deactivated. Please contact administrator");
                    return View(model);
                }

                // Check if account is locked out
                if (await _userManager.IsLockedOutAsync(user))
                {
                    var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                    var remainingTime = lockoutEnd?.Subtract(DateTimeOffset.UtcNow);

                    if (remainingTime?.TotalMinutes > 0)
                    {
                        ModelState.AddModelError("", $"Account is locked. Try again in {Math.Ceiling(remainingTime.Value.TotalMinutes)} minutes");
                        return View(model);
                    }
                }

                // Attempt to sign in
                var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User {Email} logged in successfully", model.Email);

                    // Get user roles and redirect accordingly
                    var roles = await _userManager.GetRolesAsync(user);

                    if (roles.Contains("Admin"))
                        return RedirectToAction("Dashboard", "Admin");
                    else if (roles.Contains("AgentSaisie"))
                        return RedirectToAction("AgentDashboard", "AgentSaisie");
                    else if (roles.Contains("ResponsableCatering"))
                        return RedirectToAction("CateringDashboard", "ResponsableCatering");
                    else if (roles.Contains("DGF"))
                        return RedirectToAction("Index", "FinancialDashboard");
                    else
                        return RedirectToAction("Index", "Home");
                }
                else if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account {Email} locked out", model.Email);
                    ModelState.AddModelError("", "Account locked due to multiple failed login attempts. Please try again later");
                    return View(model);
                }
                else if (result.IsNotAllowed)
                {
                    _logger.LogWarning("User {Email} login not allowed", model.Email);
                    ModelState.AddModelError("", "Login not allowed. Please contact administrator");
                    return View(model);
                }
                else
                {
                    // Invalid password
                    _logger.LogWarning("Invalid password attempt for user: {Email}", model.Email);

                    // Get failed attempts count
                    var failedAttempts = await _userManager.GetAccessFailedCountAsync(user);
                    var maxAttempts = _userManager.Options.Lockout.MaxFailedAccessAttempts;
                    var remainingAttempts = maxAttempts - failedAttempts;

                    if (remainingAttempts <= 3 && remainingAttempts > 0)
                    {
                        ModelState.AddModelError("", $"Invalid password. {remainingAttempts} attempts remaining before account lockout");
                    }
                    else
                    {
                        ModelState.AddModelError("", " Email ou mot de passe incorrect");
                    }

                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login attempt for email: {Email}", model?.Email);
                ModelState.AddModelError("", "An unexpected error occurred. Please try again");
                return View(model);
            }
        }

        public async Task<IActionResult> Logout()
        {
            try
            {
                var userEmail = User?.Identity?.Name;
                await _signInManager.SignOutAsync();

                _logger.LogInformation("User {Email} logged out", userEmail);

                // Clear cache headers
                Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
                Response.Headers["Pragma"] = "no-cache";
                Response.Headers["Expires"] = "0";

                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return RedirectToAction("Login", "Account");
            }
        }

        public async Task<IActionResult> ProfileInfo()
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    var user = await _userManager.FindByNameAsync(User.Identity.Name);
                    if (user != null)
                    {
                        var model = new UserManagementViewModel
                        {
                            NomPrenom = user.NomPrenom,
                            Email = user.Email,
                            Telephone = user.Telephone
                        };
                        return View(model);
                    }
                }
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading profile info");
                return RedirectToAction("Login");
            }
        }

        [HttpGet]
        public async Task<IActionResult> UpdateProfile()
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading update profile page");
                TempData["ErrorMessage"] = "Error loading profile. Please try again";
                return RedirectToAction("ProfileInfo");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UserManagementViewModel model)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(model.NomPrenom))
                {
                    ModelState.AddModelError("NomPrenom", "Name is required");
                }

                if (string.IsNullOrWhiteSpace(model.Email))
                {
                    ModelState.AddModelError("Email", "Email is required");
                }

                if (string.IsNullOrWhiteSpace(model.Telephone))
                {
                    ModelState.AddModelError("Telephone", "Phone number is required");
                }

                // Validate email format
                if (!string.IsNullOrWhiteSpace(model.Email) && !IsValidEmail(model.Email))
                {
                    ModelState.AddModelError("Email", "Please enter a valid email address");
                }

                // Validate phone number
                if (!string.IsNullOrWhiteSpace(model.Telephone) && !IsValidPhoneNumber(model.Telephone))
                {
                    ModelState.AddModelError("Telephone", "Phone number must be exactly 8 digits");
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login");
                }

                // Check if email is being changed and if it already exists
                if (user.Email != model.Email)
                {
                    var existingUser = await _userManager.FindByEmailAsync(model.Email);
                    if (existingUser != null && existingUser.Id != user.Id)
                    {
                        ModelState.AddModelError("Email", "This email address is already in use");
                        return View(model);
                    }
                }

                // Update user properties
                user.NomPrenom = model.NomPrenom.Trim();
                user.Email = model.Email.Trim().ToLower();
                user.UserName = model.Email.Trim().ToLower(); // Keep username in sync with email
                user.Telephone = model.Telephone.Trim();

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    await _signInManager.RefreshSignInAsync(user);
                    TempData["SuccessMessage"] = "Profile updated successfully!";
                    return RedirectToAction("AccountSettings", "Admin");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                ModelState.AddModelError("", "An unexpected error occurred while updating your profile");
                return View(model);
            }
        }

        // Helper Methods
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var emailAttribute = new EmailAddressAttribute();
                return emailAttribute.IsValid(email);
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            var digitsOnly = new string(phoneNumber.Where(char.IsDigit).ToArray());
            return digitsOnly.Length == 8;
        }
    }
}
