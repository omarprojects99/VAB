using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using VAB.Data;
using VAB.Hubs;
using VAB.Models;
using VAB.ViewModel;

namespace VAB.Controllers
{
    [Authorize(Roles = "ResponsableCatering")]
    public class ResponsableCateringController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<AgentSaisieController> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;

        public ResponsableCateringController(ApplicationDbContext context, UserManager<User> userManager, SignInManager<User> signInManager, ILogger<AgentSaisieController> logger, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _hubContext = hubContext;

        }

        public IActionResult CateringDashboard()
        {
            return View();
        }
        public IActionResult EtatDesOffresArrivee()
        {
            return View();
        }
        public IActionResult EtatDesOffresDepart()
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
        public IActionResult CompareCatring()
        {
            return View();
        }
        public IActionResult CompareOffersReports()
        {
            return View();
        }
        public IActionResult Compare()
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
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ValidateEtatDesVentesArrivee: Invalid model for ID={Id}", request.Id);
                    return Json(new { success = false, error = "Requête invalide." });
                }

                var etat = await _context.EtatDesVentesArrivee
                    .FirstOrDefaultAsync(e => e.Id == request.Id);

                if (etat == null)
                {
                    _logger.LogWarning("ValidateEtatDesVentesArrivee: No Etat found for ID={Id}", request.Id);
                    return Json(new { success = false, error = "État des Ventes non trouvé." });
                }

                if (etat.IsValidated)
                {
                    _logger.LogWarning("ValidateEtatDesVentesArrivee: Etat ID={Id} is already validated", request.Id);
                    return Json(new { success = false, error = "État déjà validé." });
                }

                etat.IsValidated = true;
                await _context.SaveChangesAsync();

                if (!string.IsNullOrEmpty(etat.AgentSaisieId))
                {
                    var message = $"État de vente numéro {etat.Numero} est validé.";
                    _logger.LogInformation("Sending notification to AgentSaisieId={AgentSaisieId}, Message={Message}", etat.AgentSaisieId, message);
                    await _hubContext.Clients.User(etat.AgentSaisieId).SendAsync("ReceiveNotification", message);
                    _context.Notifications.Add(new Notification
                    {
                        UserId = etat.AgentSaisieId,
                        Name = message,
                        CreatedAt = System.DateTime.UtcNow,
                        IsRead = false
                    });
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Notification saved for AgentSaisieId={AgentSaisieId}, Etat ID={Id}", etat.AgentSaisieId, request.Id);
                }
                else
                {
                    _logger.LogWarning("ValidateEtatDesVentesArrivee: AgentSaisieId is null for Etat ID={Id}", request.Id);
                }

                return Json(new { success = true, message = "État validé avec succès." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ValidateEtatDesVentesArrivee Error: ID={Id}", request.Id);
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
        [HttpGet]
        public async Task<IActionResult> GetEtatDesOffresArrivee()
        {
            _logger.LogInformation("Fetching all EtatDesOffresArrivee");
            try
            {
                var offres = await _context.EtatDesOffresArrivee
                    .Include(e => e.ArticleOffres)
                    .ThenInclude(ao => ao.Article)
                    .Select(e => new
                    {
                        id = e.Id,
                        numero = e.Numero,
                        dateEdition = e.DateEdition,
                        totalItemsSold = e.ArticleOffres.Sum(ao => ao.QuantiteOffert),
                        totalRevenue = e.ArticleOffres.Sum(ao => ao.QuantiteOffert * ao.Article.PrixUnitaireHT),
                        totalEncaisse = e.ArticleOffres.Sum(ao => ao.QuantiteOffert * ao.Article.PrixUnitaireHT), // Adjust as needed
                        isValidated = e.IsValidated
                    })
                    .ToListAsync();
                return Json(offres);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching EtatDesOffresArrivee");
                return StatusCode(500, "Erreur lors de la récupération des états des offres.");
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetEtatDesOffresDepart()
        {
            _logger.LogInformation("Fetching all EtatDesOffresDepart");
            try
            {
                var offres = await _context.EtatDesOffresDepart
                    .Include(e => e.ArticleOffreDepart)
                    .ThenInclude(ao => ao.Article)
                    .Select(e => new
                    {
                        id = e.Id,
                        numero = e.Numero,
                        dateEdition = e.DateEdition,
                        totalItemsSold = e.ArticleOffreDepart.Sum(ao => ao.QuantiteOffert),
                        totalRevenue = e.ArticleOffreDepart.Sum(ao => ao.QuantiteOffert * ao.Article.PrixUnitaireHT),
                        totalEncaisse = e.ArticleOffreDepart.Sum(ao => ao.QuantiteOffert * ao.Article.PrixUnitaireHT), // Adjust as needed
                        isValidated = e.IsValidated
                    })
                    .ToListAsync();
                return Json(offres);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching EtatDesOffresDepart");
                return StatusCode(500, "Erreur lors de la récupération des états des offres.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ValidateEtatDesOffresArrivee([FromBody] ValidateEtatRequest request)
        {
            try
            {
                var etat = await _context.EtatDesOffresArrivee
                    .FirstOrDefaultAsync(e => e.Id == request.Id);

                if (etat == null)
                {
                    Console.WriteLine($"ValidateEtatDesOffresArrivee: No Etat found for ID={request.Id}");
                    return Json(new { success = false, error = "État des Offres non trouvé." });
                }

                if (etat.IsValidated)
                {
                    Console.WriteLine($"ValidateEtatDesOffresArrivee: Etat ID={request.Id} is already validated");
                    return Json(new { success = false, error = "État déjà validé." });
                }

                etat.IsValidated = true;
                await _context.SaveChangesAsync();

                Console.WriteLine($"ValidateEtatDesOffresArrivee: Successfully validated Etat ID={request.Id}");
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ValidateEtatDesOffresArrivee Error: ID={request.Id}, Message={ex.Message}, StackTrace={ex.StackTrace}");
                return Json(new { success = false, error = "Erreur serveur lors de la validation." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ValidateEtatDesOffresDepart([FromBody] ValidateEtatRequest request)
        {
            try
            {
                var etat = await _context.EtatDesOffresDepart
                    .FirstOrDefaultAsync(e => e.Id == request.Id);

                if (etat == null)
                {
                    Console.WriteLine($"ValidateEtatDesOffresDepart: No Etat found for ID={request.Id}");
                    return Json(new { success = false, error = "État des Offres non trouvé." });
                }

                if (etat.IsValidated)
                {
                    Console.WriteLine($"ValidateEtatDesOffresDepart: Etat ID={request.Id} is already validated");
                    return Json(new { success = false, error = "État déjà validé." });
                }

                etat.IsValidated = true;
                await _context.SaveChangesAsync();

                Console.WriteLine($"ValidateEtatDesOffresDepart: Successfully validated Etat ID={request.Id}");
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ValidateEtatDesOffresDepart Error: ID={request.Id}, Message={ex.Message}, StackTrace={ex.StackTrace}");
                return Json(new { success = false, error = "Erreur serveur lors de la validation." });
            }
        }
        [HttpGet]
        public async Task<IActionResult> ViewEtatDesOffresArrivee(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _logger.LogWarning("ViewEtatDesOffresArrivee: User not authenticated");
                    return Unauthorized("Utilisateur non connecté.");
                }

                var etat = await _context.EtatDesOffresArrivee
                    .Include(e => e.ArticleOffres)
                    .ThenInclude(ao => ao.Article)
                    .ThenInclude(a => a.Categorie)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (etat == null)
                {
                    _logger.LogWarning($"ViewEtatDesOffresArrivee: No Etat found for ID={id}");
                    return NotFound("État des Offres non trouvé.");
                }

                var vm = new EtatDesOffresArriveeViewModel
                {
                    Id = etat.Id,
                    Numero = etat.Numero , // Handle null Numero
                    DateEdition = etat.DateEdition,
                    IsValidated = etat.IsValidated,
                    ArticleOffres = etat.ArticleOffres?.Select(ao => new ArticleOffreViewModel
                    {
                        ArticleId = ao.ArticleId,
                        IdArticle = ao.Article?.IdArticle ?? 0,
                        Description = ao.Article?.Description ?? "Unknown",
                        CategorieId = ao.Article?.CategorieId ?? 0,
                        CategorieDescription = ao.Article?.Categorie?.Description ?? "Unknown",
                        QuantiteDotation = ao.QuantiteDotation,
                        QuantiteComplementaire = ao.QuantiteComplementaire,
                        QuantiteCasse = ao.QuantiteCasse,
                        QuantiteOffert = ao.QuantiteOffert,
                        RetourConsig = ao.RetourConsig
                        // No need to set TotalEmbarque, Deficite, or Restant; they are computed
                    }).ToList() ?? new List<ArticleOffreViewModel>()
                };

                _logger.LogInformation($"ViewEtatDesOffresArrivee: Successfully loaded Etat ID={id}");
                return PartialView("ViewEtatDesOffresArrivee", vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ViewEtatDesOffresArrivee Error: ID={id}");
                return StatusCode(500, "Erreur serveur lors du chargement des détails.");
            }
        }
        [HttpGet]
        public async Task<IActionResult> ViewEtatDesOffresDepart(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _logger.LogWarning("ViewEtatDesOffresArrivee: User not authenticated");
                    return Unauthorized("Utilisateur non connecté.");
                }

                var etat = await _context.EtatDesOffresDepart
                    .Include(e => e.ArticleOffreDepart)
                    .ThenInclude(ao => ao.Article)
                    .ThenInclude(a => a.Categorie)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (etat == null)
                {
                    _logger.LogWarning($"ViewEtatDesOffresArrivee: No Etat found for ID={id}");
                    return NotFound("État des Offres non trouvé.");
                }

                var vm = new EtatDesOffresDepartViewModel
                {
                    Id = etat.Id,
                    Numero = etat.Numero, // Handle null Numero
                    DateEdition = etat.DateEdition,
                    IsValidated = etat.IsValidated,
                    ArticleOffreDepart = etat.ArticleOffreDepart?.Select(ao => new ArticleOffreDepartViewModel
                    {
                        ArticleId = ao.ArticleId,
                        IdArticle = ao.Article?.IdArticle ?? 0,
                        Description = ao.Article?.Description ?? "Unknown",
                        CategorieId = ao.Article?.CategorieId ?? 0,
                        CategorieDescription = ao.Article?.Categorie?.Description ?? "Unknown",
                        QuantiteDotation = ao.QuantiteDotation,
                        QuantiteComplementaire = ao.QuantiteComplementaire,
                        QuantiteCasse = ao.QuantiteCasse,
                        QuantiteOffert = ao.QuantiteOffert,
                        RetourConsig = ao.RetourConsig
                        // No need to set TotalEmbarque, Deficite, or Restant; they are computed
                    }).ToList() ?? new List<ArticleOffreDepartViewModel>()
                };

                _logger.LogInformation($"ViewEtatDesOffresDepart: Successfully loaded Etat ID={id}");
                return PartialView("ViewEtatDesOffresDepart", vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ViewEtatDesOffresDepart Error: ID={id}");
                return StatusCode(500, "Erreur serveur lors du chargement des détails.");
            }
        }
        [HttpPost]
        public async Task<IActionResult> CompareSalesReports(string numero, bool saveComparison = false)
        {
            try
            {
                if (string.IsNullOrEmpty(numero))
                {
                    return Json(new { success = false, message = "Report number is required." });
                }

                var arrivee = await _context.EtatDesVentesArrivee
                    .Include(e => e.ArticleVentes)
                        .ThenInclude(a => a.Article)
                    .FirstOrDefaultAsync(e => e.Numero == numero);

                var depart = await _context.EtatDesVentesDepart
                    .Include(e => e.ArticleVentesDepart)
                        .ThenInclude(a => a.Article)
                    .FirstOrDefaultAsync(e => e.Numero == numero);

                if (arrivee == null || depart == null)
                {
                    return Json(new { success = false, message = "One or both reports not found." });
                }

                var articles = new List<ComparisonArticleDto>();
                var articleDict = arrivee.ArticleVentes?.ToDictionary(a => a.ArticleId, a => a) ?? new Dictionary<int, ArticleVente>();

                foreach (var depArticle in depart.ArticleVentesDepart ?? Enumerable.Empty<ArticleVenteDepart>())
                {
                    var articleComparison = new ComparisonArticleDto
                    {
                        ArticleId = depArticle?.ArticleId ?? 0,
                        IdArticle = depArticle?.Article?.IdArticle ?? 0,
                        Description = depArticle?.Article?.Description ?? "Unknown",
                        QuantiteVenduMatch = false,
                        QuantiteVenduArrivee = 0,
                        QuantiteVenduDepart = depArticle?.QuantiteVendu ?? 0,
                        PrixUnitaireMatch = false,
                        PrixUnitaireArrivee = 0.0m,
                        PrixUnitaireDepart = depArticle?.PrixUnitaireHT ?? 0.0m,
                        ValeurMatch = false,
                        ValeurArrivee = 0.0m,
                        ValeurDepart = (depArticle?.QuantiteVendu ?? 0) * (depArticle?.PrixUnitaireHT ?? 0.0m)
                    };

                    if (articleDict.TryGetValue(depArticle?.ArticleId ?? 0, out var arrArticle))
                    {
                        articleComparison = new ComparisonArticleDto
                        {
                            ArticleId = depArticle?.ArticleId ?? 0,
                            IdArticle = depArticle?.Article?.IdArticle ?? arrArticle?.Article?.IdArticle ?? 0,
                            Description = arrArticle?.Article?.Description ?? depArticle?.Article?.Description ?? "Unknown",
                            QuantiteVenduMatch = arrArticle?.QuantiteVendu == depArticle?.QuantiteVendu,
                            QuantiteVenduArrivee = arrArticle?.QuantiteVendu ?? 0,
                            QuantiteVenduDepart = depArticle?.QuantiteVendu ?? 0,
                            PrixUnitaireMatch = arrArticle?.PrixUnitaireHT == depArticle?.PrixUnitaireHT,
                            PrixUnitaireArrivee = arrArticle?.PrixUnitaireHT ?? 0.0m,
                            PrixUnitaireDepart = depArticle?.PrixUnitaireHT ?? 0.0m,
                            ValeurMatch = (arrArticle?.QuantiteVendu * arrArticle?.PrixUnitaireHT) == (depArticle?.QuantiteVendu * depArticle?.PrixUnitaireHT),
                            ValeurArrivee = (arrArticle?.QuantiteVendu ?? 0) * (arrArticle?.PrixUnitaireHT ?? 0.0m),
                            ValeurDepart = (depArticle?.QuantiteVendu ?? 0) * (depArticle?.PrixUnitaireHT ?? 0.0m)
                        };
                    }

                    articles.Add(articleComparison);
                }

                var comparison = new
                {
                    success = true,
                    articles = articles,
                    totals = new
                    {
                        totalEncaisseMatch = arrivee.TotalEncaisse == depart.TotalEncaisse,
                        totalEncaisseArrivee = arrivee.TotalEncaisse,
                        totalEncaisseDepart = depart.TotalEncaisse,
                        ecartPlusMatch = arrivee.EcartPlus == depart.EcartPlus,
                        ecartPlusArrivee = arrivee.EcartPlus,
                        ecartPlusDepart = depart.EcartPlus,
                        ecartMoinsMatch = arrivee.EcartMoins == depart.EcartMoins,
                        ecartMoinsArrivee = arrivee.EcartMoins,
                        ecartMoinsDepart = depart.EcartMoins
                    }
                };

                if (saveComparison)
                {
                    var salesComparison = new SalesComparison
                    {
                        Numero = numero,
                        ComparisonDate = DateTime.UtcNow,
                        TotalEncaisseArrivee = arrivee.TotalEncaisse,
                        TotalEncaisseDepart = depart.TotalEncaisse,
                        TotalEncaisseMatch = arrivee.TotalEncaisse == depart.TotalEncaisse,
                        EcartPlusArrivee = arrivee.EcartPlus,
                        EcartPlusDepart = depart.EcartPlus,
                        EcartPlusMatch = arrivee.EcartPlus == depart.EcartPlus,
                        EcartMoinsArrivee = arrivee.EcartMoins,
                        EcartMoinsDepart = depart.EcartMoins,
                        EcartMoinsMatch = arrivee.EcartMoins == depart.EcartMoins,
                        Articles = articles.Select(a => new ComparisonArticle
                        {
                            ArticleId = a.ArticleId,
                            IdArticle = a.IdArticle,
                            Description = a.Description ?? "Unknown",
                            QuantiteVenduArrivee = a.QuantiteVenduArrivee,
                            QuantiteVenduDepart = a.QuantiteVenduDepart,
                            QuantiteVenduMatch = a.QuantiteVenduMatch,
                            PrixUnitaireArrivee = a.PrixUnitaireArrivee,
                            PrixUnitaireDepart = a.PrixUnitaireDepart,
                            PrixUnitaireMatch = a.PrixUnitaireMatch,
                            ValeurArrivee = a.ValeurArrivee,
                            ValeurDepart = a.ValeurDepart,
                            ValeurMatch = a.ValeurMatch
                        }).ToList()
                    };

                    _context.SalesComparisons.Add(salesComparison);
                    await _context.SaveChangesAsync();
                }

                return Json(comparison);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CompareSalesReports: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new { success = false, message = $"Server error: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CompareOffersReports(string numero, bool saveComparison = false)
        {
            try
            {
                if (string.IsNullOrEmpty(numero))
                {
                    return Json(new { success = false, message = "Report number is required." });
                }

                var arrivee = await _context.EtatDesOffresArrivee
                    .Include(e => e.ArticleOffres)
                        .ThenInclude(a => a.Article)
                    .FirstOrDefaultAsync(e => e.Numero == numero);

                var depart = await _context.EtatDesOffresDepart
                    .Include(e => e.ArticleOffreDepart)
                        .ThenInclude(a => a.Article)
                    .FirstOrDefaultAsync(e => e.Numero == numero);

                if (arrivee == null || depart == null)
                {
                    return Json(new { success = false, message = "One or both reports not found." });
                }

                var articles = new List<ComparisonOfferArticleDto>();
                var articleDict = arrivee.ArticleOffres?.ToDictionary(a => a.ArticleId, a => a) ?? new Dictionary<int, ArticleOffre>();

                foreach (var depArticle in depart.ArticleOffreDepart ?? Enumerable.Empty<ArticleOffreDepart>())
                {
                    var articleComparison = new ComparisonOfferArticleDto
                    {
                        ArticleId = depArticle?.ArticleId ?? 0,
                        IdArticle = depArticle?.Article?.IdArticle ?? 0,
                        Description = depArticle?.Article?.Description ?? "Unknown",
                        QuantiteDotationMatch = false,
                        QuantiteDotationArrivee = 0,
                        QuantiteDotationDepart = depArticle?.QuantiteDotation ?? 0,
                        QuantiteComplementaireMatch = false,
                        QuantiteComplementaireArrivee = 0,
                        QuantiteComplementaireDepart = depArticle?.QuantiteComplementaire ?? 0,
                        QuantiteCasseMatch = false,
                        QuantiteCasseArrivee = 0,
                        QuantiteCasseDepart = depArticle?.QuantiteCasse ?? 0,
                        QuantiteOffertMatch = false,
                        QuantiteOffertArrivee = 0,
                        QuantiteOffertDepart = depArticle?.QuantiteOffert ?? 0,
                        RetourConsigMatch = false,
                        RetourConsigArrivee = 0,
                        RetourConsigDepart = depArticle?.RetourConsig ?? 0
                    };

                    if (articleDict.TryGetValue(depArticle?.ArticleId ?? 0, out var arrArticle))
                    {
                        articleComparison = new ComparisonOfferArticleDto
                        {
                            ArticleId = depArticle?.ArticleId ?? 0,
                            IdArticle = depArticle?.Article?.IdArticle ?? arrArticle?.Article?.IdArticle ?? 0,
                            Description = arrArticle?.Article?.Description ?? depArticle?.Article?.Description ?? "Unknown",
                            QuantiteDotationMatch = arrArticle?.QuantiteDotation == depArticle?.QuantiteDotation,
                            QuantiteDotationArrivee = arrArticle?.QuantiteDotation ?? 0,
                            QuantiteDotationDepart = depArticle?.QuantiteDotation ?? 0,
                            QuantiteComplementaireMatch = arrArticle?.QuantiteComplementaire == depArticle?.QuantiteComplementaire,
                            QuantiteComplementaireArrivee = arrArticle?.QuantiteComplementaire ?? 0,
                            QuantiteComplementaireDepart = depArticle?.QuantiteComplementaire ?? 0,
                            QuantiteCasseMatch = arrArticle?.QuantiteCasse == depArticle?.QuantiteCasse,
                            QuantiteCasseArrivee = arrArticle?.QuantiteCasse ?? 0,
                            QuantiteCasseDepart = depArticle?.QuantiteCasse ?? 0,
                            QuantiteOffertMatch = arrArticle?.QuantiteOffert == depArticle?.QuantiteOffert,
                            QuantiteOffertArrivee = arrArticle?.QuantiteOffert ?? 0,
                            QuantiteOffertDepart = depArticle?.QuantiteOffert ?? 0,
                            RetourConsigMatch = arrArticle?.RetourConsig == depArticle?.RetourConsig,
                            RetourConsigArrivee = arrArticle?.RetourConsig ?? 0,
                            RetourConsigDepart = depArticle?.RetourConsig ?? 0
                        };
                    }

                    articles.Add(articleComparison);
                }

                var comparison = new
                {
                    success = true,
                    articles
                };

                if (saveComparison)
                {
                    var offersComparison = new OffersComparison
                    {
                        Numero = numero,
                        ComparisonDate = DateTime.UtcNow,
                        Articles = articles.Select(a => new ComparisonOfferArticle
                        {
                            ArticleId = a.ArticleId,
                            IdArticle = a.IdArticle,
                            Description = a.Description ?? "Unknown",
                            QuantiteDotationArrivee = a.QuantiteDotationArrivee,
                            QuantiteDotationDepart = a.QuantiteDotationDepart,
                            QuantiteDotationMatch = a.QuantiteDotationMatch,
                            QuantiteComplementaireArrivee = a.QuantiteComplementaireArrivee,
                            QuantiteComplementaireDepart = a.QuantiteComplementaireDepart,
                            QuantiteComplementaireMatch = a.QuantiteComplementaireMatch,
                            QuantiteCasseArrivee = a.QuantiteCasseArrivee,
                            QuantiteCasseDepart = a.QuantiteCasseDepart,
                            QuantiteCasseMatch = a.QuantiteCasseMatch,
                            QuantiteOffertArrivee = a.QuantiteOffertArrivee,
                            QuantiteOffertDepart = a.QuantiteOffertDepart,
                            QuantiteOffertMatch = a.QuantiteOffertMatch,
                            RetourConsigArrivee = a.RetourConsigArrivee,
                            RetourConsigDepart = a.RetourConsigDepart,
                            RetourConsigMatch = a.RetourConsigMatch
                        }).ToList()
                    };

                    _context.OffersComparisons.Add(offersComparison);
                    await _context.SaveChangesAsync();
                }

                return Json(comparison);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CompareOffersReports: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new { success = false, message = $"Server error: {ex.Message}" });
            }
        }
        [HttpGet]
        public async Task<IActionResult> ListComparisons()
        {
            var comparisons = await _context.SalesComparisons
                .OrderByDescending(c => c.ComparisonDate)
                .ToListAsync();
            return View(comparisons);
        }

        [HttpGet]
        public async Task<IActionResult> ViewComparison(int id)
        {
            var comparison = await _context.SalesComparisons
                .Include(c => c.Articles)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comparison == null)
            {
                return NotFound();
            }

            return View(comparison);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteComparison(int id)
        {
            var comparison = await _context.SalesComparisons.FindAsync(id);
            if (comparison == null)
            {
                return Json(new { success = false, message = "Comparison not found." });
            }

            _context.SalesComparisons.Remove(comparison);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Comparison deleted successfully." });
        }
        [HttpGet]
        public async Task<IActionResult> ViewOffersComparison(int id)
        {
            var comparison = await _context.OffersComparisons
                .Include(c => c.Articles)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comparison == null)
            {
                return NotFound();
            }

            return View(comparison);
        }

        [HttpGet]
        public async Task<IActionResult> ListOffersComparisons()
        {
            var comparisons = await _context.OffersComparisons.ToListAsync();
            return View(comparisons);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteOffersComparison(int id)
        {
            try
            {
                var comparison = await _context.OffersComparisons
                    .Include(c => c.Articles)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (comparison == null)
                {
                    return Json(new { success = false, message = "Comparison not found." });
                }

                _context.ComparisonOfferArticles.RemoveRange(comparison.Articles);
                _context.OffersComparisons.Remove(comparison);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Comparison deleted successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteOffersComparison: {ex.Message}\n{ex.StackTrace}");
                return Json(new { success = false, message = $"Error deleting comparison: {ex.Message}" });
            }
        }

      




















































    }

    public class ValidateEtatRequest
    {
        public int Id { get; set; }
    }
}