using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using VAB.Data;
using VAB.Models;
using VAB.ViewModel;

namespace VAB.Controllers
{
    [Authorize(Roles = "ResponsableCatering")]
    public class ResponsableCateringController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public ResponsableCateringController(UserManager<User> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IActionResult CateringDashboard()
        {
            return View();
        }
        public IActionResult EtatDesVentesArrivee()
        {
            return View();
        }
        public IActionResult EtatDesVentesDepart()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetEtatDesVentesArrivee()
        {
            try
            {
                var etats = _context.EtatDesVentesArrivee
                    .Include(e => e.AgentSaisie)
                    .Include(e => e.ArticleVentes)
                    .Select(e => new
                    {
                        e.Id,
                        numero = e.Numero ?? "N/A",
                        dateEdition = e.DateEdition,
                        agentSaisie = e.AgentSaisie != null && !string.IsNullOrEmpty(e.AgentSaisie.NomPrenom) ? e.AgentSaisie.NomPrenom : "Unknown",
                        totalItemsSold = e.ArticleVentes != null ? e.ArticleVentes.Sum(av => av.QuantiteVendu) : 0,
                        totalRevenue = e.ArticleVentes != null ? e.ArticleVentes.Sum(av => av.QuantiteVendu * av.PrixUnitaireHT) : 0m,
                        totalEncaisse = e.TotalEncaisse ?? 0m,
                        isValidated = e.IsValidated
                    })
                    .ToList();
                Console.WriteLine($"GetEtatDesVentesArrivee: Retrieved {etats.Count} records");
                return Json(etats);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetEtatDesVentesArrivee Error: Message={ex.Message}, StackTrace={ex.StackTrace}");
                return StatusCode(500, new { error = "Erreur serveur lors de la récupération des états." });
            }
        }
        public IActionResult GetEtatDesVentesDepart()
        {
            try
            {
                var etats = _context.EtatDesVentesDepart
                    .Include(e => e.AgentSaisie)
                    .Include(e => e.ArticleVentesDepart)
                    .Select(e => new
                    {
                        e.Id,
                        numero = e.Numero ?? "N/A",
                        dateEdition = e.DateEdition,
                        agentSaisie = e.AgentSaisie != null && !string.IsNullOrEmpty(e.AgentSaisie.NomPrenom) ? e.AgentSaisie.NomPrenom : "Unknown",
                        totalItemsSold = e.ArticleVentesDepart != null ? e.ArticleVentesDepart.Sum(av => av.QuantiteVendu) : 0,
                        totalRevenue = e.ArticleVentesDepart != null ? e.ArticleVentesDepart.Sum(av => av.QuantiteVendu * av.PrixUnitaireHT) : 0m,
                        totalEncaisse = e.TotalEncaisse ?? 0m,
                        isValidated = e.IsValidated
                    })
                    .ToList();
                Console.WriteLine($"GetEtatDesVentesDepart: Retrieved {etats.Count} records");
                return Json(etats);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetEtatDesVentesDepart Error: Message={ex.Message}, StackTrace={ex.StackTrace}");
                return StatusCode(500, new { error = "Erreur serveur lors de la récupération des états." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewEtatDesVentesArrivee(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    Console.WriteLine("ViewEtatDesVentesArrivee: User not authenticated");
                    return Unauthorized("Utilisateur non connecté.");
                }

                var etat = await _context.EtatDesVentesArrivee
                    .Include(e => e.FeuilleEtatDesVentes)
                    .ThenInclude(f => f.FeuilleDeLignes)
                    .Include(e => e.Equipage)
                    .ThenInclude(e => e.Cc1)
                    .Include(e => e.Equipage)
                    .ThenInclude(e => e.Cc2)
                    .Include(e => e.Equipage)
                    .ThenInclude(e => e.Pnc1)
                    .Include(e => e.Equipage)
                    .ThenInclude(e => e.Pnc2)
                    .Include(e => e.Equipage)
                    .ThenInclude(e => e.PncEquipages)
                    .ThenInclude(pe => pe.Pnc)
                    .Include(e => e.ArticleVentes)
                    .ThenInclude(a => a.Article)
                    .ThenInclude(a => a.Categorie)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (etat == null)
                {
                    Console.WriteLine($"ViewEtatDesVentesArrivee: No Etat found for ID={id}");
                    return NotFound("État des Ventes non trouvé.");
                }

                var vol = await _context.Vols
                    .FirstOrDefaultAsync(v => v.EtatDesVentesArriveeId == id);

                var articleVentes = new List<ArticleVenteViewModel>();
                if (etat.ArticleVentes != null)
                {
                    foreach (var av in etat.ArticleVentes)
                    {
                        articleVentes.Add(new ArticleVenteViewModel
                        {
                            ArticleId = av.ArticleId,
                            IdArticle = av.Article?.IdArticle ?? 0,
                            Description = av.Article?.Description ?? "Unknown",
                            CategorieId = av.Article?.CategorieId ?? 0,
                            CategorieDescription = av.Article?.Categorie?.Description ?? "Unknown",
                            QuantiteDotation = av.QuantiteDotation,
                            QuantiteComplementaire = av.QuantiteComplementaire,
                            QuantiteCasse = av.QuantiteCasse,
                            QuantiteOffert = av.QuantiteOffert,
                            QuantiteVendu = av.QuantiteVendu,
                            PrixUnitaireHT = av.PrixUnitaireHT
                        });
                    }
                }

                var vm = new EtatDesVentesArriveeViewModel
                {
                    Id = etat.Id,
                    Numero = etat.Numero ?? "N/A",
                    DateEdition = etat.DateEdition,
                    AgentSaisieId = etat.AgentSaisieId,
                    AgentSaisieName = etat.AgentSaisie?.NomPrenom ?? "Unknown",
                    FeuilleDeLignesNumeros = etat.FeuilleEtatDesVentes?.Select(f => f.FeuilleDeLignes?.Numero ?? 0).ToList() ?? new List<int>(),
                    Cc1Name = etat.Equipage?.Cc1 != null ? $"{etat.Equipage.Cc1.Nom} {etat.Equipage.Cc1.Prenom}" : "N/A",
                    Cc1Matricule = etat.Equipage?.Cc1?.Matricule ?? "N/A",
                    Cc2Name = etat.Equipage?.Cc2 != null ? $"{etat.Equipage.Cc2.Nom} {etat.Equipage.Cc2.Prenom}" : null,
                    Cc2Matricule = etat.Equipage?.Cc2?.Matricule,
                    Pnc1Name = etat.Equipage?.Pnc1 != null ? $"{etat.Equipage.Pnc1.Nom} {etat.Equipage.Pnc1.Prenom}" : "N/A",
                    Pnc1Matricule = etat.Equipage?.Pnc1?.Matricule ?? "N/A",
                    Pnc2Name = etat.Equipage?.Pnc2 != null ? $"{etat.Equipage.Pnc2.Nom} {etat.Equipage.Pnc2.Prenom}" : null,
                    Pnc2Matricule = etat.Equipage?.Pnc2?.Matricule,
                    DynamicPncs = etat.Equipage?.PncEquipages?.Select(pe => new DynamicPnc
                    {
                        Name = pe.Pnc != null ? $"{pe.Pnc.Nom} {pe.Pnc.Prenom}" : "N/A",
                        Matricule = pe.Pnc?.Matricule ?? "N/A"
                    }).ToList() ?? new List<DynamicPnc>(),
                    FlightNumber = vol?.Numero ?? 0,
                    FlightDate = vol?.Date ?? DateTime.MinValue,
                    Destination = vol?.Destination,
                    Aircraft = vol?.Aircraft,
                    FlightTime = vol?.Heure ?? TimeSpan.Zero,
                    FlightType = vol?.TypeVol,
                    TotalPassengers = vol?.NombrePassagers ?? 0,
                    TotalEncaisse = etat.TotalEncaisse,
                    EcartPlus = etat.EcartPlus,
                    EcartMoins = etat.EcartMoins,
                    ArticleVentes = articleVentes,
                    IsValidated = etat.IsValidated
                };

                Console.WriteLine($"ViewEtatDesVentesArrivee: Successfully loaded Etat ID={id}");
                return PartialView("ViewEtatDesVentesArrivee", vm);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ViewEtatDesVentesArrivee Error: ID={id}, Message={ex.Message}, StackTrace={ex.StackTrace}");
                return StatusCode(500, "Erreur serveur lors du chargement des détails.");
            }
        }
        [HttpGet]
        public async Task<IActionResult> ViewEtatDesVentesDepart(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    Console.WriteLine("ViewEtatDesVentesDepart: User not authenticated");
                    return Unauthorized("Utilisateur non connecté.");
                }

                var etat = await _context.EtatDesVentesDepart
                    .Include(e => e.FeuilleEtatDesVentes)
                    .ThenInclude(f => f.FeuilleDeLignes)
                    .Include(e => e.Equipage)
                    .ThenInclude(e => e.Cc1)
                    .Include(e => e.Equipage)
                    .ThenInclude(e => e.Cc2)
                    .Include(e => e.Equipage)
                    .ThenInclude(e => e.Pnc1)
                    .Include(e => e.Equipage)
                    .ThenInclude(e => e.Pnc2)
                    .Include(e => e.Equipage)
                    .ThenInclude(e => e.PncEquipages)
                    .ThenInclude(pe => pe.Pnc)
                    .Include(e => e.ArticleVentesDepart)
                    .ThenInclude(a => a.Article)
                    .ThenInclude(a => a.Categorie)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (etat == null)
                {
                    Console.WriteLine($"ViewEtatDesVentesDepart: No Etat found for ID={id}");
                    return NotFound("État des Ventes non trouvé.");
                }

                var vol = await _context.Vols
                    .FirstOrDefaultAsync(v => v.EtatDesVentesDepartId == id);

                var articleVentes = new List<ArticleVenteViewModel>();
                if (etat.ArticleVentesDepart != null)
                {
                    foreach (var av in etat.ArticleVentesDepart)
                    {
                        articleVentes.Add(new ArticleVenteViewModel
                        {
                            ArticleId = av.ArticleId,
                            IdArticle = av.Article?.IdArticle ?? 0,
                            Description = av.Article?.Description ?? "Unknown",
                            CategorieId = av.Article?.CategorieId ?? 0,
                            CategorieDescription = av.Article?.Categorie?.Description ?? "Unknown",
                            QuantiteDotation = av.QuantiteDotation,
                            QuantiteComplementaire = av.QuantiteComplementaire,
                            QuantiteCasse = av.QuantiteCasse,
                            QuantiteOffert = av.QuantiteOffert,
                            QuantiteVendu = av.QuantiteVendu,
                            PrixUnitaireHT = av.PrixUnitaireHT
                        });
                    }
                }

                var vm = new EtatDesVentesArriveeViewModel
                {
                    Id = etat.Id,
                    Numero = etat.Numero ?? "N/A",
                    DateEdition = etat.DateEdition,
                    AgentSaisieId = etat.AgentSaisieId,
                    AgentSaisieName = etat.AgentSaisie?.NomPrenom ?? "Unknown",
                    FeuilleDeLignesNumeros = etat.FeuilleEtatDesVentes?.Select(f => f.FeuilleDeLignes?.Numero ?? 0).ToList() ?? new List<int>(),
                    Cc1Name = etat.Equipage?.Cc1 != null ? $"{etat.Equipage.Cc1.Nom} {etat.Equipage.Cc1.Prenom}" : "N/A",
                    Cc1Matricule = etat.Equipage?.Cc1?.Matricule ?? "N/A",
                    Cc2Name = etat.Equipage?.Cc2 != null ? $"{etat.Equipage.Cc2.Nom} {etat.Equipage.Cc2.Prenom}" : null,
                    Cc2Matricule = etat.Equipage?.Cc2?.Matricule,
                    Pnc1Name = etat.Equipage?.Pnc1 != null ? $"{etat.Equipage.Pnc1.Nom} {etat.Equipage.Pnc1.Prenom}" : "N/A",
                    Pnc1Matricule = etat.Equipage?.Pnc1?.Matricule ?? "N/A",
                    Pnc2Name = etat.Equipage?.Pnc2 != null ? $"{etat.Equipage.Pnc2.Nom} {etat.Equipage.Pnc2.Prenom}" : null,
                    Pnc2Matricule = etat.Equipage?.Pnc2?.Matricule,
                    DynamicPncs = etat.Equipage?.PncEquipages?.Select(pe => new DynamicPnc
                    {
                        Name = pe.Pnc != null ? $"{pe.Pnc.Nom} {pe.Pnc.Prenom}" : "N/A",
                        Matricule = pe.Pnc?.Matricule ?? "N/A"
                    }).ToList() ?? new List<DynamicPnc>(),
                    FlightNumber = vol?.Numero ?? 0,
                    FlightDate = vol?.Date ?? DateTime.MinValue,
                    Destination = vol?.Destination,
                    Aircraft = vol?.Aircraft,
                    FlightTime = vol?.Heure ?? TimeSpan.Zero,
                    FlightType = vol?.TypeVol,
                    TotalPassengers = vol?.NombrePassagers ?? 0,
                    TotalEncaisse = etat.TotalEncaisse,
                    EcartPlus = etat.EcartPlus,
                    EcartMoins = etat.EcartMoins,
                    ArticleVentes = articleVentes,
                    IsValidated = etat.IsValidated
                };

                Console.WriteLine($"ViewEtatDesVentesDepart: Successfully loaded Etat ID={id}");
                return PartialView("ViewEtatDesVentesDepart", vm);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ViewEtatDesVentesDepart Error: ID={id}, Message={ex.Message}, StackTrace={ex.StackTrace}");
                return StatusCode(500, "Erreur serveur lors du chargement des détails.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ValidateEtatDesVentesArrivee([FromBody] ValidateEtatRequest request)
        {
            try
            {
                var etat = await _context.EtatDesVentesArrivee
                    .FirstOrDefaultAsync(e => e.Id == request.Id);

                if (etat == null)
                {
                    Console.WriteLine($"ValidateEtatDesVentesArrivee: No Etat found for ID={request.Id}");
                    return Json(new { success = false, error = "État des Ventes non trouvé." });
                }

                if (etat.IsValidated)
                {
                    Console.WriteLine($"ValidateEtatDesVentesArrivee: Etat ID={request.Id} is already validated");
                    return Json(new { success = false, error = "État déjà validé." });
                }

                etat.IsValidated = true;
                await _context.SaveChangesAsync();

                Console.WriteLine($"ValidateEtatDesVentesArrivee: Successfully validated Etat ID={request.Id}");
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ValidateEtatDesVentesArrivee Error: ID={request.Id}, Message={ex.Message}, StackTrace={ex.StackTrace}");
                return Json(new { success = false, error = "Erreur serveur lors de la validation." });
            }
        }
        [HttpPost]
        public async Task<IActionResult> ValidateEtatDesVentesDepart([FromBody] ValidateEtatRequest request)
        {
            try
            {
                var etat = await _context.EtatDesVentesDepart
                    .FirstOrDefaultAsync(e => e.Id == request.Id);

                if (etat == null)
                {
                    Console.WriteLine($"ValidateEtatDesVentesDepart: No Etat found for ID={request.Id}");
                    return Json(new { success = false, error = "État des Ventes non trouvé." });
                }

                if (etat.IsValidated)
                {
                    Console.WriteLine($"ValidateEtatDesVentesDepart: Etat ID={request.Id} is already validated");
                    return Json(new { success = false, error = "État déjà validé." });
                }

                etat.IsValidated = true;
                await _context.SaveChangesAsync();

                Console.WriteLine($"ValidateEtatDesVentesDepart: Successfully validated Etat ID={request.Id}");
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ValidateEtatDesVentesDepart Error: ID={request.Id}, Message={ex.Message}, StackTrace={ex.StackTrace}");
                return Json(new { success = false, error = "Erreur serveur lors de la validation." });
            }
        }
    }

    public class ValidateEtatRequest
    {
        public int Id { get; set; }
    }
}