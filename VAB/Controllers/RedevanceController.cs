using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VAB.Data;
using VAB.Models;
using VAB.Services;

namespace VAB.Controllers
{
    [Authorize]
    public class RedevanceController : Controller
    {
        private readonly IRedevanceService _redevanceService;
        private readonly IRedevanceAnalyticsService _analyticsService;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;


        public RedevanceController(
            IRedevanceService redevanceService,
            IRedevanceAnalyticsService analyticsService,
            UserManager<User> userManager,
            ApplicationDbContext context
            )
        {
            _redevanceService = redevanceService;
            _analyticsService = analyticsService;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var redevances = await _redevanceService.GetAllRedevancesAsync();
            var currentYear = DateTime.Now.Year;
            ViewBag.TotalAnnee = await _redevanceService.GetTotalRedevanceForYearAsync(currentYear);
            return View(redevances);
        }

        public async Task<IActionResult> Dashboard(int annee = 0)
        {
            if (annee == 0) annee = DateTime.Now.Year;

            var analytics = await _analyticsService.GetAdvancedAnalyticsAsync(annee);
            ViewBag.Annee = annee;
            ViewBag.CurrentPage = "Dashboard";
            return View(analytics);
        }

        public async Task<IActionResult> MonthlyDetails(int mois, int annee)
        {
            // Get the redevance for this specific month
            var redevance = await _context.Redevances
                .Include(r => r.RedevanceDetails)
                    .ThenInclude(rd => rd.EtatDesVentesArrivee)
                        .ThenInclude(e => e.ArticleVentes)
                            .ThenInclude(av => av.Article)
                .Include(r => r.CalculePar)
                .FirstOrDefaultAsync(r => r.Mois == mois && r.Annee == annee);

            if (redevance == null)
            {
                // If no redevance exists, show what data is available for calculation
                var etatsVentes = await _analyticsService.GetValidatedStatesForPeriodAsync(mois, annee);
                ViewBag.EtatsVentesDisponibles = etatsVentes;
                ViewBag.CanCalculate = etatsVentes.Any();
            }

            // Get additional analytics for this month
            var topProducts = await _analyticsService.GetTopSellingProductsAsync(mois, annee, 10);
            var monthlyTrend = await _analyticsService.GetMonthlyTrendsAsync(annee);
            var currentMonth = monthlyTrend.FirstOrDefault(m => m.Mois == mois);

            ViewBag.TopProducts = topProducts;
            ViewBag.CurrentMonth = currentMonth;
            ViewBag.Mois = mois;
            ViewBag.Annee = annee;
            ViewBag.MoisNom = GetMonthName(mois);
            ViewBag.CurrentPage = "MonthlyDetails";

            return View(redevance);
        }

        public async Task<IActionResult> Analytics(int annee = 0)
        {
            if (annee == 0) annee = DateTime.Now.Year;

            var analytics = await _analyticsService.GetAdvancedAnalyticsAsync(annee);
            ViewBag.Annee = annee;
            ViewBag.CurrentPage = "Analytics";
            return View(analytics);
        }

        [HttpGet]
        public async Task<IActionResult> GetChartData(string type, int annee)
        {
            var data = await _analyticsService.GetChartDataAsync(type, annee);
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetRealTimeStats()
        {
            var stats = await _analyticsService.GetRealTimeStatsAsync();
            return Json(stats);
        }

        public IActionResult Calculate()
        {
            ViewBag.Mois = DateTime.Now.Month;
            ViewBag.Annee = DateTime.Now.Year;
            ViewBag.CurrentPage = "Calculate";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Calculate(int mois, int annee)
        {
            var userId = _userManager.GetUserId(User);
            var success = await _redevanceService.CalculateRedevanceAsync(mois, annee, userId);

            if (success)
            {
                TempData["Success"] = "Redevance calculée avec succès!";
                return RedirectToAction(nameof(MonthlyDetails), new { mois, annee });
            }

            TempData["Error"] = "Erreur lors du calcul ou aucune donnée trouvée.";
            return View();
        }

        public async Task<IActionResult> Details(int id)
        {
            var redevance = await _redevanceService.GetRedevanceAsync(id);
            if (redevance == null)
                return NotFound();

            ViewBag.CurrentPage = "Details";
            return View(redevance);
        }

        [HttpPost]
        public async Task<IActionResult> Validate(int id)
        {
            var success = await _redevanceService.ValidateRedevanceAsync(id);

            if (success)
                TempData["Success"] = "Redevance validée!";
            else
                TempData["Error"] = "Impossible de valider.";

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _redevanceService.DeleteRedevanceAsync(id);

            if (success)
                TempData["Success"] = "Redevance supprimée!";
            else
                TempData["Error"] = "Impossible de supprimer.";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Export(int annee, string format = "excel")
        {
            var analytics = await _analyticsService.GetAdvancedAnalyticsAsync(annee);
            return Json(analytics);
        }

        private string GetMonthName(int month)
        {
            return month switch
            {
                1 => "Janvier",
                2 => "Février",
                3 => "Mars",
                4 => "Avril",
                5 => "Mai",
                6 => "Juin",
                7 => "Juillet",
                8 => "Août",
                9 => "Septembre",
                10 => "Octobre",
                11 => "Novembre",
                12 => "Décembre",
                _ => "Inconnu"
            };
        }
    }
}
