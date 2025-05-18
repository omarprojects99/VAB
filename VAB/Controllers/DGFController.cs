using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VAB.Data;
using VAB.Models;

namespace VAB.Controllers
{
    public class DGFController : Controller
    {
        [Authorize(Roles = "DGF")]
        public IActionResult DGFDashboard()
        {
            return View();
        }

    }
}
