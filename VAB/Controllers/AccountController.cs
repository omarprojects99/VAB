using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VAB.Models;
using VAB.ViewModel;


namespace VAB.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && user.IsActive)
            {
                var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    //return RedirectToAction("Dashboard", "Admin");
                    // Get the user's roles from the database
                    var roles = await _userManager.GetRolesAsync(user);

                    
                    if (roles.Contains("Admin"))
                        return RedirectToAction("Dashboard", "Admin");
                    else if (roles.Contains("AgentSaisie"))
                        return RedirectToAction("AgentForm", "AgentSaisie");
                    else if (roles.Contains("Chef"))
                        return RedirectToAction("Dashboard", "ResponsableCatering");
                    else if (roles.Contains("DGF"))
                        return RedirectToAction("Dashboard", "DGF");
                    else
                        return RedirectToAction("Index", "Home");
                }
            }
            ModelState.AddModelError("", "Invalid Login Attempt");
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
            return RedirectToAction("Login", "Account");
        }
    }
}
