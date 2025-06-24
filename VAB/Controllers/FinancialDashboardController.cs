using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VAB.Data;
using VAB.ViewModel;
using Microsoft.AspNetCore.Authorization;
using System.Globalization;
using VAB.Models;

namespace VAB.Controllers
{
    [Authorize(Roles = "DGF,Admin")]
    public class FinancialDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FinancialDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Default action - this will handle /FinancialDashboard and /FinancialDashboard/Index
        public async Task<IActionResult> Index(DateTime? dateDebut, DateTime? dateFin)
        {
            // Set default date range if not provided (last 12 months)
            if (!dateDebut.HasValue)
                dateDebut = DateTime.Now.AddMonths(-12);
            if (!dateFin.HasValue)
                dateFin = DateTime.Now;

            var viewModel = new FinancialDashboardViewModel
            {
                DateDebut = dateDebut,
                DateFin = dateFin
            };

            try
            {
                // Commission Statistics
                var commissionQuery = _context.PncCommissionCalculations
                    .Include(c => c.EtatDesVentesArrivee)
                    .Include(c => c.PncCommissionDetails)
                        .ThenInclude(d => d.Pnc)
                    .Where(c => c.DateCalcul >= dateDebut && c.DateCalcul <= dateFin);

                var commissions = await commissionQuery.ToListAsync();

                viewModel.TotalCommissionCalculations = commissions.Count;
                viewModel.TotalCommissionsAmount = commissions.Sum(c =>
                    c.PncCommissionDetails.Sum(d => d.CommissionTotale));
                viewModel.ActivePncsWithCommissions = commissions
                    .SelectMany(c => c.PncCommissionDetails)
                    .Select(d => d.PncId)
                    .Distinct()
                    .Count();
                viewModel.AverageCommissionPerPnc = viewModel.ActivePncsWithCommissions > 0
                    ? viewModel.TotalCommissionsAmount / viewModel.ActivePncsWithCommissions
                    : 0;

                // Redevance Statistics - Fixed query
                // Get all redevances first, then filter in memory
                var allRedevances = await _context.Redevances
                    .Include(r => r.RedevanceDetails)
                    .ToListAsync();

                // Filter by date range in memory
                var redevances = allRedevances
                    .Where(r =>
                    {
                        var redevanceDate = new DateTime(r.Annee, r.Mois, 1);
                        return redevanceDate >= dateDebut.Value.Date && redevanceDate <= dateFin.Value.Date;
                    })
                    .ToList();

                viewModel.TotalRedevanceRecords = redevances.Count;
                viewModel.TotalRedevanceAmount = redevances.Sum(r => r.MontantRedevance);

                // Current and last month redevance
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;
                var lastMonth = currentMonth == 1 ? 12 : currentMonth - 1;
                var lastMonthYear = currentMonth == 1 ? currentYear - 1 : currentYear;

                viewModel.CurrentMonthRedevance = redevances
                    .Where(r => r.Mois == currentMonth && r.Annee == currentYear)
                    .Sum(r => r.MontantRedevance);

                viewModel.LastMonthRedevance = redevances
                    .Where(r => r.Mois == lastMonth && r.Annee == lastMonthYear)
                    .Sum(r => r.MontantRedevance);

                viewModel.RedevanceGrowthPercentage = viewModel.LastMonthRedevance > 0
                    ? ((viewModel.CurrentMonthRedevance - viewModel.LastMonthRedevance) / viewModel.LastMonthRedevance) * 100
                    : 0;

                // Trou de Caisse Statistics
                var trouQuery = _context.TrouDeCaisses
                    .Include(t => t.EtatDesVentesArrivee)
                    .Include(t => t.PncResponsable)
                    .Where(t => t.DateDetection >= dateDebut && t.DateDetection <= dateFin);

                var trous = await trouQuery.ToListAsync();

                viewModel.TotalTrousDeCaisse = trous.Count;
                viewModel.TotalMontantManquant = trous.Sum(t => t.MontantManquant);
                viewModel.TrousEnAttente = trous.Count(t => t.Statut == "En Attente");
                viewModel.TrousAppliques = trous.Count(t => t.EstApplique);
                viewModel.AverageTrouAmount = viewModel.TotalTrousDeCaisse > 0
                    ? viewModel.TotalMontantManquant / viewModel.TotalTrousDeCaisse
                    : 0;

                // Recent Commissions
                viewModel.RecentCommissions = commissions
                    .OrderByDescending(c => c.DateCalcul)
                    .Take(10)
                    .SelectMany(c => c.PncCommissionDetails.Select(d => new RecentCommissionViewModel
                    {
                        Id = c.Id,
                        PncName = $"{d.Pnc.Nom} {d.Pnc.Prenom}",
                        NumeroEtat = c.EtatDesVentesArrivee.Numero,
                        DateCalcul = c.DateCalcul,
                        CommissionTotale = d.CommissionTotale,
                        CalculatedBy = c.CalculatedBy
                    }))
                    .OrderByDescending(r => r.DateCalcul)
                    .Take(10)
                    .ToList();

                // Recent Trous de Caisse
                viewModel.RecentTrousDeCaisse = trous
                    .OrderByDescending(t => t.DateDetection)
                    .Take(10)
                    .Select(t => new RecentTrouDeCaisseViewModel
                    {
                        Id = t.Id,
                        PncResponsable = $"{t.PncResponsable.Nom} {t.PncResponsable.Prenom}",
                        NumeroEtat = t.EtatDesVentesArrivee.Numero,
                        DateDetection = t.DateDetection,
                        MontantManquant = t.MontantManquant,
                        Statut = t.Statut,
                        EstApplique = t.EstApplique
                    })
                    .ToList();

                // Monthly Trends
                viewModel.CommissionTrends = await GetCommissionTrends(dateDebut.Value, dateFin.Value);
                viewModel.RedevanceTrends = await GetRedevanceTrends(dateDebut.Value, dateFin.Value);

                // Top PNC Performance
                viewModel.TopPncs = await GetTopPncPerformance(dateDebut.Value, dateFin.Value);

                // Monthly Revenue Summary
                viewModel.MonthlyRevenues = await GetMonthlyRevenues(dateDebut.Value, dateFin.Value);
            }
            catch (Exception ex)
            {
                // Log the error and provide default values
                Console.WriteLine($"Error loading dashboard data: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                // Initialize with default values to prevent null reference errors
                viewModel.RecentCommissions = new List<RecentCommissionViewModel>();
                viewModel.RecentTrousDeCaisse = new List<RecentTrouDeCaisseViewModel>();
                viewModel.CommissionTrends = new List<MonthlyTrendViewModel>();
                viewModel.RedevanceTrends = new List<MonthlyTrendViewModel>();
                viewModel.TopPncs = new List<TopPncPerformance>();
                viewModel.MonthlyRevenues = new List<MonthlyRevenueViewModel>();

                // You might want to add a message to display to the user
                ViewBag.ErrorMessage = "Erreur lors du chargement des données. Certaines informations peuvent être indisponibles.";
            }

            return View(viewModel);
        }

        // Test action to verify controller is working
        public IActionResult Test()
        {
            return Json(new { message = "FinancialDashboard Controller is working!", timestamp = DateTime.Now });
        }

        private async Task<List<MonthlyTrendViewModel>> GetCommissionTrends(DateTime dateDebut, DateTime dateFin)
        {
            try
            {
                var commissions = await _context.PncCommissionCalculations
                    .Include(c => c.PncCommissionDetails)
                    .Where(c => c.DateCalcul >= dateDebut && c.DateCalcul <= dateFin)
                    .ToListAsync();

                return commissions
                    .GroupBy(c => new { c.DateCalcul.Year, c.DateCalcul.Month })
                    .Select(g => new MonthlyTrendViewModel
                    {
                        Annee = g.Key.Year,
                        Mois = g.Key.Month,
                        MoisNom = GetMonthName(g.Key.Month),
                        Montant = g.Sum(c => c.PncCommissionDetails.Sum(d => d.CommissionTotale)),
                        Count = g.Count()
                    })
                    .OrderBy(t => t.Annee)
                    .ThenBy(t => t.Mois)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCommissionTrends: {ex.Message}");
                return new List<MonthlyTrendViewModel>();
            }
        }

        private async Task<List<MonthlyTrendViewModel>> GetRedevanceTrends(DateTime dateDebut, DateTime dateFin)
        {
            try
            {
                // Get all redevances and filter in memory to avoid EF Core translation issues
                var allRedevances = await _context.Redevances.ToListAsync();

                var filteredRedevances = allRedevances
                    .Where(r =>
                    {
                        var redevanceDate = new DateTime(r.Annee, r.Mois, 1);
                        return redevanceDate >= dateDebut.Date && redevanceDate <= dateFin.Date;
                    })
                    .ToList();

                return filteredRedevances
                    .Select(r => new MonthlyTrendViewModel
                    {
                        Annee = r.Annee,
                        Mois = r.Mois,
                        MoisNom = GetMonthName(r.Mois),
                        Montant = r.MontantRedevance,
                        Count = r.NombreEtats
                    })
                    .OrderBy(t => t.Annee)
                    .ThenBy(t => t.Mois)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetRedevanceTrends: {ex.Message}");
                return new List<MonthlyTrendViewModel>();
            }
        }

        private async Task<List<TopPncPerformance>> GetTopPncPerformance(DateTime dateDebut, DateTime dateFin)
        {
            try
            {
                // Get all PNC commission data for the period
                var pncCommissionData = await _context.PncCommissionDetails
                    .Include(d => d.Pnc)
                    .Include(d => d.PncCommissionCalculation)
                    .Where(d => d.PncCommissionCalculation.DateCalcul >= dateDebut &&
                       d.PncCommissionCalculation.DateCalcul <= dateFin)
                    .ToListAsync();

                // Get all trous de caisse data for the period
                var trousData = await _context.TrouDeCaisses
                    .Include(t => t.PncResponsable)
                    .Where(t => t.DateDetection >= dateDebut && t.DateDetection <= dateFin && t.EstApplique)
                    .ToListAsync();

                // Group commission data by PNC
                var pncCommissionGroups = pncCommissionData
                    .GroupBy(d => new {
                        d.PncId,
                        d.Pnc.Nom,
                        d.Pnc.Prenom,
                        d.Pnc.Matricule,
                        d.Pnc.Telephone
                    })
                    .ToList();

                // Group trous data by PNC
                var pncTrousGroups = trousData
                    .GroupBy(t => new {
                        PncId = t.PncResponsableId,
                        t.PncResponsable.Nom,
                        t.PncResponsable.Prenom
                    })
                    .ToDictionary(g => g.Key.PncId, g => g.Sum(t => t.MontantManquant));

                var topPncs = new List<TopPncPerformance>();

                foreach (var group in pncCommissionGroups)
                {
                    var totalCommissions = group.Sum(d => d.CommissionTotale);
                    var totalTrous = pncTrousGroups.ContainsKey(group.Key.PncId)
                        ? pncTrousGroups[group.Key.PncId]
                        : 0;

                    // Find best month for this PNC
                    var bestMonth = group
                        .GroupBy(d => new { d.PncCommissionCalculation.DateCalcul.Year, d.PncCommissionCalculation.DateCalcul.Month })
                        .OrderByDescending(mg => mg.Sum(d => d.CommissionTotale))
                        .FirstOrDefault();

                    var bestMonthName = bestMonth != null
                        ? $"{GetMonthName(bestMonth.Key.Month)} {bestMonth.Key.Year}"
                        : "N/A";

                    // Calculate performance rate (commissions vs trous ratio)
                    var tauxPerformance = totalCommissions > 0
                        ? Math.Round(((totalCommissions - totalTrous) / totalCommissions) * 100, 2)
                        : 0;

                    topPncs.Add(new TopPncPerformance
                    {
                        PncId = group.Key.PncId,
                        PncInfo = $"{group.Key.Nom} {group.Key.Prenom}, {group.Key.Matricule}",
                        Nom = group.Key.Nom,
                        Prenom = group.Key.Prenom,
                        Matricule = group.Key.Matricule,
                        Telephone = group.Key.Telephone,
                        TotalCommissions = totalCommissions,
                        TotalTrousDeCaisse = totalTrous,
                        PerformanceNette = totalCommissions - totalTrous,
                        NombreCalculs = group.Count(),
                        CommissionMoyenneParCalcul = group.Count() > 0 ? totalCommissions / group.Count() : 0,
                        MeilleurMois = bestMonthName,
                        TauxPerformance = tauxPerformance
                    });
                }

                return topPncs
                    .OrderByDescending(p => p.PerformanceNette)
                    .Take(10)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetTopPncPerformance: {ex.Message}");
                return new List<TopPncPerformance>();
            }
        }

        private async Task<List<MonthlyRevenueViewModel>> GetMonthlyRevenues(DateTime dateDebut, DateTime dateFin)
        {
            try
            {
                var months = new List<MonthlyRevenueViewModel>();
                var current = new DateTime(dateDebut.Year, dateDebut.Month, 1);
                var end = new DateTime(dateFin.Year, dateFin.Month, 1);

                // Get all data first to avoid multiple database calls
                var allCommissions = await _context.PncCommissionCalculations
                    .Include(c => c.PncCommissionDetails)
                    .Where(c => c.DateCalcul >= dateDebut && c.DateCalcul <= dateFin)
                    .ToListAsync();

                var allRedevances = await _context.Redevances
                    .Where(r => r.Annee >= dateDebut.Year && r.Annee <= dateFin.Year)
                    .ToListAsync();

                var allTrous = await _context.TrouDeCaisses
                    .Where(t => t.DateDetection >= dateDebut && t.DateDetection <= dateFin && t.EstApplique)
                    .ToListAsync();

                while (current <= end)
                {
                    var monthStart = current;
                    var monthEnd = current.AddMonths(1).AddDays(-1);

                    var commissions = allCommissions
                        .Where(c => c.DateCalcul >= monthStart && c.DateCalcul <= monthEnd)
                        .Sum(c => c.PncCommissionDetails.Sum(d => d.CommissionTotale));

                    var redevances = allRedevances
                        .Where(r => r.Annee == current.Year && r.Mois == current.Month)
                        .Sum(r => r.MontantRedevance);

                    var trous = allTrous
                        .Where(t => t.DateDetection >= monthStart && t.DateDetection <= monthEnd)
                        .Sum(t => t.MontantManquant);

                    months.Add(new MonthlyRevenueViewModel
                    {
                        Mois = current.Month,
                        Annee = current.Year,
                        MoisNom = GetMonthName(current.Month),
                        TotalCommissions = commissions,
                        TotalRedevances = redevances,
                        TotalTrousDeCaisse = trous,
                        NetRevenue = commissions + redevances - trous
                    });

                    current = current.AddMonths(1);
                }

                return months;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetMonthlyRevenues: {ex.Message}");
                return new List<MonthlyRevenueViewModel>();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetChartData(string chartType, DateTime? dateDebut, DateTime? dateFin)
        {
            if (!dateDebut.HasValue) dateDebut = DateTime.Now.AddMonths(-12);
            if (!dateFin.HasValue) dateFin = DateTime.Now;

            try
            {
                switch (chartType.ToLower())
                {
                    case "commission-trends":
                        var commissionTrends = await GetCommissionTrends(dateDebut.Value, dateFin.Value);
                        return Json(new
                        {
                            labels = commissionTrends.Select(t => $"{t.MoisNom} {t.Annee}").ToArray(),
                            data = commissionTrends.Select(t => t.Montant).ToArray()
                        });

                    case "redevance-trends":
                        var redevanceTrends = await GetRedevanceTrends(dateDebut.Value, dateFin.Value);
                        return Json(new
                        {
                            labels = redevanceTrends.Select(t => $"{t.MoisNom} {t.Annee}").ToArray(),
                            data = redevanceTrends.Select(t => t.Montant).ToArray()
                        });

                    case "trou-status":
                        var trouStats = await _context.TrouDeCaisses
                            .Where(t => t.DateDetection >= dateDebut && t.DateDetection <= dateFin)
                            .ToListAsync();

                        var statusGroups = trouStats
                            .GroupBy(t => t.Statut)
                            .Select(g => new { Status = g.Key, Count = g.Count() })
                            .ToList();

                        return Json(new
                        {
                            labels = statusGroups.Select(s => s.Status).ToArray(),
                            data = statusGroups.Select(s => s.Count).ToArray()
                        });

                    default:
                        return BadRequest("Invalid chart type");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetChartData: {ex.Message}");
                return Json(new { error = ex.Message });
            }
        }

        // Helper method to get month names in French
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
