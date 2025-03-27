using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

using VAB.ViewModel;
using VAB.Models;

namespace VAB.Controllers
{
    [Authorize(Roles = "AgentSaisie")]
    public class AgentSaisieController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult AgentForm()
        {
            return View();
        }
    }
}
