using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAB.Data;
using VAB.Models;
using VAB.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using VAB.Models;
using VAB.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace VAB.Controllers
{
    [Authorize(Roles = "AgentSaisie")]
    public class AgentSaisieController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AgentSaisieController(ApplicationDbContext context, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;

        }

        // GET: AgentSaisie/AgentDashboard
        public IActionResult AgentDashboard()
        {
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);

            // Optional: get the first day of next month
            var startOfNextMonth = startOfMonth.AddMonths(1);

            var countThisMonth = _context.EtatDesVentesArrivee
                .Where(e => e.DateEdition >= startOfMonth && e.DateEdition < startOfNextMonth)
                .Count();

            ViewBag.CountThisMonth = countThisMonth;

            return View();
        }

        // GET: AgentSaisie/AgentForm
        public IActionResult AgentForm()
        {
            return View();
        }
        public IActionResult EtatDesVentesDepart()
        {
            return View();

        }
        public IActionResult EtatDesVentesArrivee()
        {
            return View();
        }

        public IActionResult AddEtatDesVentesDepart()
        {
            return PartialView("AddEtatDeVenteDepart", new EtatDesVentesArriveeViewModel());
        }
        //view depart
        public async Task<IActionResult> ViewEtatDesVentesDepart(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
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
                .Include(e => e.ArticleVentesDepart) // Changed from ArticlesVendus to match your model
                .ThenInclude(a => a.Article)
                .ThenInclude(a => a.Categorie)
                .FirstOrDefaultAsync(e => e.Id == id && e.AgentSaisieId == user.Id);

            if (etat == null)
            {
                return NotFound("État des Ventes non trouvé ou vous n'êtes pas autorisé à le voir.");
            }

            var vol = await _context.Vols
                .FirstOrDefaultAsync(v => v.EtatDesVentesDepartId == id);

            // Create a list of ArticleVenteViewModel objects that match the structure expected by the view
            var articleVentes = new List<ArticleVenteViewModel>();

            if (etat.ArticleVentesDepart != null)
            {
                foreach (var av in etat.ArticleVentesDepart)
                {
                    articleVentes.Add(new ArticleVenteViewModel
                    {
                        ArticleId = av.ArticleId,
                        IdArticle = av.Article.IdArticle, // Make sure this property exists in your Article model
                        Description = av.Article.Description,
                        CategorieId = av.Article.CategorieId,
                        CategorieDescription = av.Article.Categorie.Description,
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
                Numero = etat.Numero,
                DateEdition = etat.DateEdition,
                AgentSaisieId = etat.AgentSaisieId,
                AgentSaisieName = user.NomPrenom,
                FeuilleDeLignesNumeros = etat.FeuilleEtatDesVentes.Select(f => f.FeuilleDeLignes.Numero).ToList(),
                Cc1Name = $"{etat.Equipage.Cc1.Nom} {etat.Equipage.Cc1.Prenom}",
                Cc1Matricule = etat.Equipage.Cc1.Matricule,
                Cc2Name = etat.Equipage.Cc2 != null ? $"{etat.Equipage.Cc2.Nom} {etat.Equipage.Cc2.Prenom}" : null,
                Cc2Matricule = etat.Equipage.Cc2?.Matricule,
                Pnc1Name = $"{etat.Equipage.Pnc1.Nom} {etat.Equipage.Pnc1.Prenom}",
                Pnc1Matricule = etat.Equipage.Pnc1.Matricule,
                Pnc2Name = etat.Equipage.Pnc2 != null ? $"{etat.Equipage.Pnc2.Nom} {etat.Equipage.Pnc2.Prenom}" : null,
                Pnc2Matricule = etat.Equipage.Pnc2?.Matricule,
                DynamicPncs = etat.Equipage.PncEquipages?.Select(pe => new DynamicPnc
                {
                    Name = $"{pe.Pnc.Nom} {pe.Pnc.Prenom}",
                    Matricule = pe.Pnc.Matricule
                }).ToList() ?? new List<DynamicPnc>(),
                // Vol details
                FlightNumber = vol?.Numero ?? 0,
                FlightDate = vol?.Date ?? DateTime.MinValue,
                Destination = vol?.Destination,
                Aircraft = vol?.Aircraft,
                FlightTime = vol?.Heure ?? TimeSpan.Zero,
                FlightType = vol?.TypeVol,
                TotalPassengers = vol?.NombrePassagers ?? 0,
                // Financial details
                TotalEncaisse = etat.TotalEncaisse,
                EcartPlus = etat.EcartPlus,
                EcartMoins = etat.EcartMoins,
                // Set the ArticleVentes property directly
                ArticleVentes = articleVentes
            };

            return PartialView("ViewEtatDeVenteDepart", vm);
        }

        //form VenteDepart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEtatDesVentesDepart(EtatDesVentesArriveeViewModel vm)
        {
            ModelState.Remove("AgentSaisieId");
            ModelState.Remove("AgentSaisieName");
            if (vm.ArticleVentes != null)
            {
                foreach (var av in vm.ArticleVentes)
                {
                    ModelState.Remove($"ArticleVentes[{vm.ArticleVentes.IndexOf(av)}].CategorieDescription");
                }
            }
            if (vm.ArticleVentes != null)
            {
                for (int i = 0; i < vm.ArticleVentes.Count; i++)
                {
                    ModelState.Remove($"ArticleVentes[{i}].PrixUnitaireHT");
                }
            }
            // After removing validation, manually set the PrixUnitaireHT from the database
            if (vm.ArticleVentes != null)
            {
                foreach (var av in vm.ArticleVentes)
                {
                    if (av.ArticleId > 0)
                    {
                        // Get the correct price from the database
                        var article = await _context.Articles.FindAsync(av.ArticleId);
                        if (article != null)
                        {
                            // Use the database value instead of the form value
                            av.PrixUnitaireHT = article.PrixUnitaireHT;
                        }
                    }
                }
            }
            // Log ArticleVentes for debugging
            Console.WriteLine($"Received ArticleVentes: Count={(vm.ArticleVentes?.Count ?? 0)}");
            if (vm.ArticleVentes != null)
            {
                foreach (var av in vm.ArticleVentes)
                {
                    Console.WriteLine($"Article: ArticleId={av.ArticleId}, IdArticle={av.IdArticle}, PrixUnitaireHT={av.PrixUnitaireHT}, QuantiteVendu={av.QuantiteVendu}");
                }
            }

            if (!ModelState.IsValid)
            {
                var errorss = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                ViewBag.ErrorMessage = "Veuillez corriger les erreurs dans le formulaire: " + string.Join("; ", errorss);
                return PartialView("AddEtatDeVenteDepart", vm);
            }

            if (await _context.EtatDesVentesDepart.AnyAsync(e => e.Numero == vm.Numero))
            {
                ViewBag.ErrorMessage = "Le numéro d'état existe déjà.";
                return PartialView("AddEtatDeVenteDepart", vm);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                ViewBag.ErrorMessage = "Utilisateur non connecté. Veuillez vous reconnecter.";
                return PartialView("AddEtatDeVenteDepart", vm);
            }

            if (vm.FeuilleDeLignesNumeros == null || !vm.FeuilleDeLignesNumeros.Any(n => n > 0))
            {
                ViewBag.ErrorMessage = "Veuillez fournir au moins un numéro de Feuille de Lignes valide.";
                return PartialView("AddEtatDeVenteDepart", vm);
            }

            var feuilles = new List<FeuilleDeLignes>();
            foreach (var numero in vm.FeuilleDeLignesNumeros)
            {
                if (numero <= 0) continue;
                var f = await _context.FeuilleDeLignes
                    .Include(f => f.Vols)
                    .FirstOrDefaultAsync(x => x.Numero == numero);
                if (f == null)
                {
                    ViewBag.ErrorMessage = $"Feuille de Lignes avec Numéro {numero} n'existe pas.";
                    return PartialView("AddEtatDeVenteDepart", vm);
                }
                feuilles.Add(f);
            }

            // Validate Vol
            Vol vol = null;
            foreach (var feuille in feuilles)
            {
                vol = feuille.Vols.FirstOrDefault(v => v.Numero == vm.FlightNumber);
                if (vol != null) break;
            }
            if (vol == null)
            {
                ViewBag.ErrorMessage = $"Le vol {vm.FlightNumber} n'existe pas dans les Feuilles de Lignes spécifiées.";
                return PartialView("AddEtatDeVenteDepart", vm);
            }

            // Log input and database values for debugging
            Console.WriteLine($"Validating Vol: Numero={vm.FlightNumber}");
            Console.WriteLine($"Input: Date={vm.FlightDate:yyyy-MM-dd}, DB: Date={vol.Date:yyyy-MM-dd}");
            Console.WriteLine($"Input: Destination={vm.Destination}, DB: Destination={vol.Destination}");
            Console.WriteLine($"Input: TypeVol={vm.FlightType}, DB: TypeVol={vol.TypeVol}");
            Console.WriteLine($"Input: Heure={vm.FlightTime}, DB: Heure={vol.Heure}");
            Console.WriteLine($"Input: NombrePassagers={vm.TotalPassengers}, DB: NombrePassagers={vol.NombrePassagers}");
            Console.WriteLine($"Input: Aircraft={vm.Aircraft ?? "null"}, DB: Aircraft={vol.Aircraft ?? "null"}");

            // Validate Vol details with case-insensitive string comparison
            var errors = new List<string>();
            if (vol.Date.Date != vm.FlightDate.Date)
                errors.Add($"Date mismatch: Input={vm.FlightDate:yyyy-MM-dd}, DB={vol.Date:yyyy-MM-dd}");
            if (!string.Equals(vol.Destination?.Trim(), vm.Destination?.Trim(), StringComparison.OrdinalIgnoreCase))
                errors.Add($"Destination mismatch: Input={vm.Destination}, DB={vol.Destination}");
            if (!string.Equals(vol.TypeVol?.Trim(), vm.FlightType?.Trim(), StringComparison.OrdinalIgnoreCase))
                errors.Add($"TypeVol mismatch: Input={vm.FlightType}, DB={vol.TypeVol}");
            if (vol.Heure != vm.FlightTime)
                errors.Add($"Heure mismatch: Input={vm.FlightTime}, DB={vol.Heure}");
            if (vol.NombrePassagers != vm.TotalPassengers)
                errors.Add($"NombrePassagers mismatch: Input={vm.TotalPassengers}, DB={vol.NombrePassagers}");
            if (vol.Aircraft != null && vm.Aircraft != null && !string.Equals(vol.Aircraft.Trim(), vm.Aircraft.Trim(), StringComparison.OrdinalIgnoreCase))
                errors.Add($"Aircraft mismatch: Input={vm.Aircraft}, DB={vol.Aircraft}");

            if (errors.Any())
            {
                ViewBag.ErrorMessage = "Les détails du vol ne correspondent pas aux informations de la Feuille de Lignes: " + string.Join("; ", errors);
                return PartialView("AddEtatDeVenteDepart", vm);
            }

            // Validate CC1
            var cc1 = await _context.Ccs.FirstOrDefaultAsync(c => c.Matricule == vm.Cc1Matricule);
            if (cc1 == null)
            {
                ViewBag.ErrorMessage = $"CC1 avec matricule {vm.Cc1Matricule} n'existe pas.";
                return PartialView("AddEtatDeVenteDepart", vm);
            }
            if (!string.Equals($"{cc1.Nom} {cc1.Prenom}".Trim(), vm.Cc1Name.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.ErrorMessage = $"Le nom du CC1 ({vm.Cc1Name}) ne correspond pas au matricule {vm.Cc1Matricule}.";
                return PartialView("AddEtatDeVenteDepart", vm);
            }

            // Validate CC2 (optional)
            Cc cc2 = null;
            if (!string.IsNullOrEmpty(vm.Cc2Name) || !string.IsNullOrEmpty(vm.Cc2Matricule))
            {
                if (string.IsNullOrEmpty(vm.Cc2Name) || string.IsNullOrEmpty(vm.Cc2Matricule))
                {
                    ViewBag.ErrorMessage = "Veuillez fournir à la fois le nom et le matricule pour CC2, ou laisser les deux vides.";
                    return PartialView("AddEtatDeVenteDepart", vm);
                }
                cc2 = await _context.Ccs.FirstOrDefaultAsync(c => c.Matricule == vm.Cc2Matricule);
                if (cc2 == null)
                {
                    ViewBag.ErrorMessage = $"CC2 avec matricule {vm.Cc2Matricule} n'existe pas.";
                    return PartialView("AddEtatDeVenteDepart", vm);
                }
                if (!string.Equals($"{cc2.Nom} {cc2.Prenom}".Trim(), vm.Cc2Name.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    ViewBag.ErrorMessage = $"Le nom du CC2 ({vm.Cc2Name}) ne correspond pas au matricule {vm.Cc2Matricule}.";
                    return PartialView("AddEtatDeVenteDepart", vm);
                }
            }

            // Validate PNC1
            var pnc1 = await _context.Pncs.FirstOrDefaultAsync(p => p.Matricule == vm.Pnc1Matricule);
            if (pnc1 == null)
            {
                ViewBag.ErrorMessage = $"PNC1 avec matricule {vm.Pnc1Matricule} n'existe pas.";
                return PartialView("AddEtatDeVenteDepart", vm);
            }
            if (!string.Equals($"{pnc1.Nom} {pnc1.Prenom}".Trim(), vm.Pnc1Name.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.ErrorMessage = $"Le nom du PNC1 ({vm.Pnc1Name}) ne correspond pas au matricule {vm.Pnc1Matricule}.";
                return PartialView("AddEtatDeVenteDepart", vm);
            }

            // Validate PNC2 (optional)
            Pnc pnc2 = null;
            if (!string.IsNullOrEmpty(vm.Pnc2Name) || !string.IsNullOrEmpty(vm.Pnc2Matricule))
            {
                if (string.IsNullOrEmpty(vm.Pnc2Name) || string.IsNullOrEmpty(vm.Pnc2Matricule))
                {
                    ViewBag.ErrorMessage = "Veuillez fournir à la fois le nom et le matricule pour PNC2, ou laisser les deux vides.";
                    return PartialView("AddEtatDeVenteDepart", vm);
                }
                pnc2 = await _context.Pncs.FirstOrDefaultAsync(p => p.Matricule == vm.Pnc2Matricule);
                if (pnc2 == null)
                {
                    ViewBag.ErrorMessage = $"PNC2 avec matricule {vm.Pnc2Matricule} n'existe pas.";
                    return PartialView("AddEtatDeVenteDepart", vm);
                }
                if (!string.Equals($"{pnc2.Nom} {pnc2.Prenom}".Trim(), vm.Pnc2Name.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    ViewBag.ErrorMessage = $"Le nom du PNC2 ({vm.Pnc2Name}) ne correspond pas au matricule {vm.Pnc2Matricule}.";
                    return PartialView("AddEtatDeVenteDepart", vm);
                }
            }

            // Validate and retrieve Dynamic PNCs
            var dynamicPncIds = new List<int>();
            if (vm.DynamicPncs != null && vm.DynamicPncs.Any())
            {
                Console.WriteLine($"Received {vm.DynamicPncs.Count} dynamic PNCs");
                var uniqueMatricules = new HashSet<string>();
                foreach (var dynamicPnc in vm.DynamicPncs)
                {
                    Console.WriteLine($"Processing PNC: Name={dynamicPnc.Name}, Matricule={dynamicPnc.Matricule}");
                    if (string.IsNullOrEmpty(dynamicPnc.Name) || string.IsNullOrEmpty(dynamicPnc.Matricule))
                    {
                        Console.WriteLine($"Skipping empty PNC: Name={dynamicPnc.Name}, Matricule={dynamicPnc.Matricule}");
                        continue;
                    }
                    if (!uniqueMatricules.Add(dynamicPnc.Matricule))
                    {
                        Console.WriteLine($"Skipping duplicate PNC matricule: {dynamicPnc.Matricule}");
                        continue;
                    }
                    var pnc = await _context.Pncs.FirstOrDefaultAsync(p => p.Matricule == dynamicPnc.Matricule);
                    if (pnc == null)
                    {
                        ViewBag.ErrorMessage = $"PNC dynamique avec matricule {dynamicPnc.Matricule} n'existe pas.";
                        return PartialView("AddEtatDeVenteDepart", vm);
                    }
                    if (!string.Equals($"{pnc.Nom} {pnc.Prenom}".Trim(), dynamicPnc.Name.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        ViewBag.ErrorMessage = $"Le nom du PNC dynamique ({dynamicPnc.Name}) ne correspond pas au matricule {dynamicPnc.Matricule}.";
                        return PartialView("AddEtatDeVenteDepart", vm);
                    }
                    dynamicPncIds.Add(pnc.PncId);
                }
                Console.WriteLine($"Unique PncIds: {string.Join(", ", dynamicPncIds)}");
            }


            // Validate ArticleVentes
            var articleVentesDepart = new List<ArticleVenteDepart>();
            if (vm.ArticleVentes != null && vm.ArticleVentes.Any())
            {
                foreach (var av in vm.ArticleVentes)
                {
                    if (av.ArticleId <= 0 || av.IdArticle <= 0)
                    {
                        Console.WriteLine($"Skipping invalid article: ArticleId={av.ArticleId}, IdArticle={av.IdArticle}");
                        continue;
                    }

                    var article = await _context.Articles
                        .Include(a => a.Categorie)
                        .FirstOrDefaultAsync(a => a.Id == av.ArticleId && a.IdArticle == av.IdArticle);
                    if (article == null)
                    {
                        ViewBag.ErrorMessage = $"L'article avec ID {av.IdArticle} n'existe pas.";
                        return PartialView("AddEtatDeVenteDepart", vm);
                    }

                    // Validate PrixUnitaireHT with tolerance
                    const decimal tolerance = 0.01m;
                    if (Math.Abs(article.PrixUnitaireHT - av.PrixUnitaireHT) > tolerance)
                    {
                        ViewBag.ErrorMessage = $"Le prix unitaire HT pour l'article {av.IdArticle} ne correspond pas (DB: {article.PrixUnitaireHT}, Input: {av.PrixUnitaireHT}).";
                        return PartialView("AddEtatDeVenteDepart", vm);
                    }

                    var articleVenteDepart = new ArticleVenteDepart
                    {
                        ArticleId = av.ArticleId,
                        QuantiteDotation = av.QuantiteDotation,
                        QuantiteComplementaire = av.QuantiteComplementaire,
                        QuantiteCasse = av.QuantiteCasse,
                        QuantiteOffert = av.QuantiteOffert,
                        QuantiteVendu = av.QuantiteVendu,
                        PrixUnitaireHT = article.PrixUnitaireHT // Use DB value for consistency
                    };
                    articleVentesDepart.Add(articleVenteDepart);
                }
            }

            if (!articleVentesDepart.Any())
            {
                ViewBag.ErrorMessage = "Veuillez ajouter au moins un article valide.";
                return PartialView("AddEtatDeVenteDepart", vm);
            }

            try
            {
                // Create Equipage
                var equipage = new Equipage
                {
                    Cc1Id = cc1.CcId,
                    Cc2Id = cc2?.CcId,
                    Pnc1Id = pnc1.PncId,
                    Pnc2Id = pnc2?.PncId
                };
                _context.Equipages.Add(equipage);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Created Equipage with ID={equipage.EquipageId}");

                // Add Dynamic PNCs to PncEquipages
                foreach (var pncId in dynamicPncIds)
                {
                    var pncEquipage = new PncEquipage
                    {
                        PncId = pncId,
                        EquipageId = equipage.EquipageId
                    };
                    _context.PncEquipages.Add(pncEquipage);
                    Console.WriteLine($"Added PncEquipage: PncId={pncId}, EquipageId={equipage.EquipageId}");
                }
                Console.WriteLine($"PncEquipages to save: {_context.ChangeTracker.Entries<PncEquipage>().Count(e => e.State == EntityState.Added)}");
                await _context.SaveChangesAsync();
                Console.WriteLine($"Saved {await _context.PncEquipages.CountAsync(pe => pe.EquipageId == equipage.EquipageId)} PncEquipages");

                // Create EtatDesVentesDepart
                var etat = new EtatDesVentesDepart
                {
                    Numero = vm.Numero,
                    DateEdition = vm.DateEdition,
                    EquipageId = equipage.EquipageId,
                    AgentSaisieId = user.Id,
                    TotalEncaisse = vm.TotalEncaisse,
                    EcartPlus = vm.EcartPlus,
                    EcartMoins = vm.EcartMoins,
                    FeuilleEtatDesVentes = new List<FeuilleEtatDesVentesDepart>(),
                    ArticleVentesDepart = articleVentesDepart
                };

                foreach (var f in feuilles)
                {
                    var junction = new FeuilleEtatDesVentesDepart
                    {
                        FeuilleDeLignes = f,
                        EtatDesVentesDepart = etat
                    };
                    etat.FeuilleEtatDesVentes.Add(junction);
                }

                foreach (var av in articleVentesDepart)
                {
                    av.EtatDesVentesDepart = etat;
                }

                _context.EtatDesVentesDepart.Add(etat);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Saved EtatDesVentesArrivee: Numero={etat.Numero}, VolId={vol.Id}");

                // Update Vol to link to EtatDesVentesArrivee
                vol.EtatDesVentesDepartId = etat.Id;
                _context.Vols.Update(vol);
                await _context.SaveChangesAsync();

                ViewBag.Success = true;
                return PartialView("AddEtatDeVenteDepart", new EtatDesVentesArriveeViewModel());
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"Database error: {dbEx.InnerException?.Message ?? dbEx.Message}");
                ViewBag.ErrorMessage = $"Erreur lors de l'enregistrement des données: {dbEx.InnerException?.Message ?? dbEx.Message}";
                return PartialView("AddEtatDeVenteDepart", vm);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error: {ex.Message}");
                ViewBag.ErrorMessage = $"Erreur lors de l'enregistrement de l'état: {ex.Message}";
                return PartialView("AddEtatDeVenteDepart", vm);
            }
        }
        //public IActionResult EtatDesVentesArrivee()
        //{
        //    var reports = new List<object>
        //    {
        //        new { Id = 1,numero = 2542525, Date = "2023-10-01", PNCName = "Salah Ali", TotalItemsSold = 10, TotalRevenue = 100.00m },
        //        new { Id = 2,numero = 236757, Date = "2023-10-02", PNCName = "Mohamed Ahmed", TotalItemsSold = 15, TotalRevenue = 150.00m }
        //    };
        //    return View(reports);
        //}
        public IActionResult EtatDesOffresDepart()
        {
            return View();
        }
        public IActionResult EtatDesOffresArrivee()
        {
            return View();
        }
        //public IActionResult AddEtatDesVentesArrivee()
        //{
        //    return PartialView("AddEtatDeVenteArrivee");
        //}
        // GET: Load the form
        //public IActionResult AddEtatDesVentesArrivee()
        // GET: Load the form
        public IActionResult AddEtatDesVentesArrivee()
        {
            return PartialView("AddEtatDeVenteArrivee", new EtatDesVentesArriveeViewModel());
        }

        // POST: Save the form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEtatDesVentesArrivee(EtatDesVentesArriveeViewModel vm)
        {
            ModelState.Remove("AgentSaisieId");
            ModelState.Remove("AgentSaisieName");
            if (vm.ArticleVentes != null)
            {
                foreach (var av in vm.ArticleVentes)
                {
                    ModelState.Remove($"ArticleVentes[{vm.ArticleVentes.IndexOf(av)}].CategorieDescription");
                }
            }
            if (vm.ArticleVentes != null)
            {
                for (int i = 0; i < vm.ArticleVentes.Count; i++)
                {
                    ModelState.Remove($"ArticleVentes[{i}].PrixUnitaireHT");
                }
            }
            // After removing validation, manually set the PrixUnitaireHT from the database
            if (vm.ArticleVentes != null)
            {
                foreach (var av in vm.ArticleVentes)
                {
                    if (av.ArticleId > 0)
                    {
                        // Get the correct price from the database
                        var article = await _context.Articles.FindAsync(av.ArticleId);
                        if (article != null)
                        {
                            // Use the database value instead of the form value
                            av.PrixUnitaireHT = article.PrixUnitaireHT;
                        }
                    }
                }
            }
            // Log ArticleVentes for debugging
            Console.WriteLine($"Received ArticleVentes: Count={(vm.ArticleVentes?.Count ?? 0)}");
            if (vm.ArticleVentes != null)
            {
                foreach (var av in vm.ArticleVentes)
                {
                    Console.WriteLine($"Article: ArticleId={av.ArticleId}, IdArticle={av.IdArticle}, PrixUnitaireHT={av.PrixUnitaireHT}, QuantiteVendu={av.QuantiteVendu}");
                }
            }

            if (!ModelState.IsValid)
            {
                var errorss = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                ViewBag.ErrorMessage = "Veuillez corriger les erreurs dans le formulaire: " + string.Join("; ", errorss);
                return PartialView("AddEtatDeVenteArrivee", vm);
            }

            if (await _context.EtatDesVentesArrivee.AnyAsync(e => e.Numero == vm.Numero))
            {
                ViewBag.ErrorMessage = "Le numéro d'état existe déjà.";
                return PartialView("AddEtatDeVenteArrivee", vm);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                ViewBag.ErrorMessage = "Utilisateur non connecté. Veuillez vous reconnecter.";
                return PartialView("AddEtatDeVenteArrivee", vm);
            }

            if (vm.FeuilleDeLignesNumeros == null || !vm.FeuilleDeLignesNumeros.Any(n => n > 0))
            {
                ViewBag.ErrorMessage = "Veuillez fournir au moins un numéro de Feuille de Lignes valide.";
                return PartialView("AddEtatDeVenteArrivee", vm);
            }

            var feuilles = new List<FeuilleDeLignes>();
            foreach (var numero in vm.FeuilleDeLignesNumeros)
            {
                if (numero <= 0) continue;
                var f = await _context.FeuilleDeLignes
                    .Include(f => f.Vols)
                    .FirstOrDefaultAsync(x => x.Numero == numero);
                if (f == null)
                {
                    ViewBag.ErrorMessage = $"Feuille de Lignes avec Numéro {numero} n'existe pas.";
                    return PartialView("AddEtatDeVenteArrivee", vm);
                }
                feuilles.Add(f);
            }

            // Validate Vol
            Vol vol = null;
            foreach (var feuille in feuilles)
            {
                vol = feuille.Vols.FirstOrDefault(v => v.Numero == vm.FlightNumber);
                if (vol != null) break;
            }
            if (vol == null)
            {
                ViewBag.ErrorMessage = $"Le vol {vm.FlightNumber} n'existe pas dans les Feuilles de Lignes spécifiées.";
                return PartialView("AddEtatDeVenteArrivee", vm);
            }

            // Log input and database values for debugging
            Console.WriteLine($"Validating Vol: Numero={vm.FlightNumber}");
            Console.WriteLine($"Input: Date={vm.FlightDate:yyyy-MM-dd}, DB: Date={vol.Date:yyyy-MM-dd}");
            Console.WriteLine($"Input: Destination={vm.Destination}, DB: Destination={vol.Destination}");
            Console.WriteLine($"Input: TypeVol={vm.FlightType}, DB: TypeVol={vol.TypeVol}");
            Console.WriteLine($"Input: Heure={vm.FlightTime}, DB: Heure={vol.Heure}");
            Console.WriteLine($"Input: NombrePassagers={vm.TotalPassengers}, DB: NombrePassagers={vol.NombrePassagers}");
            Console.WriteLine($"Input: Aircraft={vm.Aircraft ?? "null"}, DB: Aircraft={vol.Aircraft ?? "null"}");

            // Validate Vol details with case-insensitive string comparison
            var errors = new List<string>();
            if (vol.Date.Date != vm.FlightDate.Date)
                errors.Add($"Date mismatch: Input={vm.FlightDate:yyyy-MM-dd}, DB={vol.Date:yyyy-MM-dd}");
            if (!string.Equals(vol.Destination?.Trim(), vm.Destination?.Trim(), StringComparison.OrdinalIgnoreCase))
                errors.Add($"Destination mismatch: Input={vm.Destination}, DB={vol.Destination}");
            if (!string.Equals(vol.TypeVol?.Trim(), vm.FlightType?.Trim(), StringComparison.OrdinalIgnoreCase))
                errors.Add($"TypeVol mismatch: Input={vm.FlightType}, DB={vol.TypeVol}");
            if (vol.Heure != vm.FlightTime)
                errors.Add($"Heure mismatch: Input={vm.FlightTime}, DB={vol.Heure}");
            if (vol.NombrePassagers != vm.TotalPassengers)
                errors.Add($"NombrePassagers mismatch: Input={vm.TotalPassengers}, DB={vol.NombrePassagers}");
            if (vol.Aircraft != null && vm.Aircraft != null && !string.Equals(vol.Aircraft.Trim(), vm.Aircraft.Trim(), StringComparison.OrdinalIgnoreCase))
                errors.Add($"Aircraft mismatch: Input={vm.Aircraft}, DB={vol.Aircraft}");

            if (errors.Any())
            {
                ViewBag.ErrorMessage = "Les détails du vol ne correspondent pas aux informations de la Feuille de Lignes: " + string.Join("; ", errors);
                return PartialView("AddEtatDeVenteArrivee", vm);
            }

            // Validate CC1
            var cc1 = await _context.Ccs.FirstOrDefaultAsync(c => c.Matricule == vm.Cc1Matricule);
            if (cc1 == null)
            {
                ViewBag.ErrorMessage = $"CC1 avec matricule {vm.Cc1Matricule} n'existe pas.";
                return PartialView("AddEtatDeVenteArrivee", vm);
            }
            if (!string.Equals($"{cc1.Nom} {cc1.Prenom}".Trim(), vm.Cc1Name.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.ErrorMessage = $"Le nom du CC1 ({vm.Cc1Name}) ne correspond pas au matricule {vm.Cc1Matricule}.";
                return PartialView("AddEtatDeVenteArrivee", vm);
            }

            // Validate CC2 (optional)
            Cc cc2 = null;
            if (!string.IsNullOrEmpty(vm.Cc2Name) || !string.IsNullOrEmpty(vm.Cc2Matricule))
            {
                if (string.IsNullOrEmpty(vm.Cc2Name) || string.IsNullOrEmpty(vm.Cc2Matricule))
                {
                    ViewBag.ErrorMessage = "Veuillez fournir à la fois le nom et le matricule pour CC2, ou laisser les deux vides.";
                    return PartialView("AddEtatDeVenteArrivee", vm);
                }
                cc2 = await _context.Ccs.FirstOrDefaultAsync(c => c.Matricule == vm.Cc2Matricule);
                if (cc2 == null)
                {
                    ViewBag.ErrorMessage = $"CC2 avec matricule {vm.Cc2Matricule} n'existe pas.";
                    return PartialView("AddEtatDeVenteArrivee", vm);
                }
                if (!string.Equals($"{cc2.Nom} {cc2.Prenom}".Trim(), vm.Cc2Name.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    ViewBag.ErrorMessage = $"Le nom du CC2 ({vm.Cc2Name}) ne correspond pas au matricule {vm.Cc2Matricule}.";
                    return PartialView("AddEtatDeVenteArrivee", vm);
                }
            }

            // Validate PNC1
            var pnc1 = await _context.Pncs.FirstOrDefaultAsync(p => p.Matricule == vm.Pnc1Matricule);
            if (pnc1 == null)
            {
                ViewBag.ErrorMessage = $"PNC1 avec matricule {vm.Pnc1Matricule} n'existe pas.";
                return PartialView("AddEtatDeVenteArrivee", vm);
            }
            if (!string.Equals($"{pnc1.Nom} {pnc1.Prenom}".Trim(), vm.Pnc1Name.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.ErrorMessage = $"Le nom du PNC1 ({vm.Pnc1Name}) ne correspond pas au matricule {vm.Pnc1Matricule}.";
                return PartialView("AddEtatDeVenteArrivee", vm);
            }

            // Validate PNC2 (optional)
            Pnc pnc2 = null;
            if (!string.IsNullOrEmpty(vm.Pnc2Name) || !string.IsNullOrEmpty(vm.Pnc2Matricule))
            {
                if (string.IsNullOrEmpty(vm.Pnc2Name) || string.IsNullOrEmpty(vm.Pnc2Matricule))
                {
                    ViewBag.ErrorMessage = "Veuillez fournir à la fois le nom et le matricule pour PNC2, ou laisser les deux vides.";
                    return PartialView("AddEtatDeVenteArrivee", vm);
                }
                pnc2 = await _context.Pncs.FirstOrDefaultAsync(p => p.Matricule == vm.Pnc2Matricule);
                if (pnc2 == null)
                {
                    ViewBag.ErrorMessage = $"PNC2 avec matricule {vm.Pnc2Matricule} n'existe pas.";
                    return PartialView("AddEtatDeVenteArrivee", vm);
                }
                if (!string.Equals($"{pnc2.Nom} {pnc2.Prenom}".Trim(), vm.Pnc2Name.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    ViewBag.ErrorMessage = $"Le nom du PNC2 ({vm.Pnc2Name}) ne correspond pas au matricule {vm.Pnc2Matricule}.";
                    return PartialView("AddEtatDeVenteArrivee", vm);
                }
            }

            // Validate and retrieve Dynamic PNCs
            var dynamicPncIds = new List<int>();
            if (vm.DynamicPncs != null && vm.DynamicPncs.Any())
            {
                Console.WriteLine($"Received {vm.DynamicPncs.Count} dynamic PNCs");
                var uniqueMatricules = new HashSet<string>();
                foreach (var dynamicPnc in vm.DynamicPncs)
                {
                    Console.WriteLine($"Processing PNC: Name={dynamicPnc.Name}, Matricule={dynamicPnc.Matricule}");
                    if (string.IsNullOrEmpty(dynamicPnc.Name) || string.IsNullOrEmpty(dynamicPnc.Matricule))
                    {
                        Console.WriteLine($"Skipping empty PNC: Name={dynamicPnc.Name}, Matricule={dynamicPnc.Matricule}");
                        continue;
                    }
                    if (!uniqueMatricules.Add(dynamicPnc.Matricule))
                    {
                        Console.WriteLine($"Skipping duplicate PNC matricule: {dynamicPnc.Matricule}");
                        continue;
                    }
                    var pnc = await _context.Pncs.FirstOrDefaultAsync(p => p.Matricule == dynamicPnc.Matricule);
                    if (pnc == null)
                    {
                        ViewBag.ErrorMessage = $"PNC dynamique avec matricule {dynamicPnc.Matricule} n'existe pas.";
                        return PartialView("AddEtatDeVenteArrivee", vm);
                    }
                    if (!string.Equals($"{pnc.Nom} {pnc.Prenom}".Trim(), dynamicPnc.Name.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        ViewBag.ErrorMessage = $"Le nom du PNC dynamique ({dynamicPnc.Name}) ne correspond pas au matricule {dynamicPnc.Matricule}.";
                        return PartialView("AddEtatDeVenteArrivee", vm);
                    }
                    dynamicPncIds.Add(pnc.PncId);
                }
                Console.WriteLine($"Unique PncIds: {string.Join(", ", dynamicPncIds)}");
            }


            // Validate ArticleVentes
            var articleVentes = new List<ArticleVente>();
            if (vm.ArticleVentes != null && vm.ArticleVentes.Any())
            {
                foreach (var av in vm.ArticleVentes)
                {
                    if (av.ArticleId <= 0 || av.IdArticle <= 0)
                    {
                        Console.WriteLine($"Skipping invalid article: ArticleId={av.ArticleId}, IdArticle={av.IdArticle}");
                        continue;
                    }

                    var article = await _context.Articles
                        .Include(a => a.Categorie)
                        .FirstOrDefaultAsync(a => a.Id == av.ArticleId && a.IdArticle == av.IdArticle);
                    if (article == null)
                    {
                        ViewBag.ErrorMessage = $"L'article avec ID {av.IdArticle} n'existe pas.";
                        return PartialView("AddEtatDeVenteArrivee", vm);
                    }

                    // Validate PrixUnitaireHT with tolerance
                    const decimal tolerance = 0.01m;
                    if (Math.Abs(article.PrixUnitaireHT - av.PrixUnitaireHT) > tolerance)
                    {
                        ViewBag.ErrorMessage = $"Le prix unitaire HT pour l'article {av.IdArticle} ne correspond pas (DB: {article.PrixUnitaireHT}, Input: {av.PrixUnitaireHT}).";
                        return PartialView("AddEtatDeVenteArrivee", vm);
                    }

                    var articleVente = new ArticleVente
                    {
                        ArticleId = av.ArticleId,
                        QuantiteDotation = av.QuantiteDotation,
                        QuantiteComplementaire = av.QuantiteComplementaire,
                        QuantiteCasse = av.QuantiteCasse,
                        QuantiteOffert = av.QuantiteOffert,
                        QuantiteVendu = av.QuantiteVendu,
                        PrixUnitaireHT = article.PrixUnitaireHT // Use DB value for consistency
                    };
                    articleVentes.Add(articleVente);
                }
            }

            if (!articleVentes.Any())
            {
                ViewBag.ErrorMessage = "Veuillez ajouter au moins un article valide.";
                return PartialView("AddEtatDeVenteArrivee", vm);
            }

            try
            {
                // Create Equipage
                var equipage = new Equipage
                {
                    Cc1Id = cc1.CcId,
                    Cc2Id = cc2?.CcId,
                    Pnc1Id = pnc1.PncId,
                    Pnc2Id = pnc2?.PncId
                };
                _context.Equipages.Add(equipage);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Created Equipage with ID={equipage.EquipageId}");

                // Add Dynamic PNCs to PncEquipages
                foreach (var pncId in dynamicPncIds)
                {
                    var pncEquipage = new PncEquipage
                    {
                        PncId = pncId,
                        EquipageId = equipage.EquipageId
                    };
                    _context.PncEquipages.Add(pncEquipage);
                    Console.WriteLine($"Added PncEquipage: PncId={pncId}, EquipageId={equipage.EquipageId}");
                }
                Console.WriteLine($"PncEquipages to save: {_context.ChangeTracker.Entries<PncEquipage>().Count(e => e.State == EntityState.Added)}");
                await _context.SaveChangesAsync();
                Console.WriteLine($"Saved {await _context.PncEquipages.CountAsync(pe => pe.EquipageId == equipage.EquipageId)} PncEquipages");

                // Create EtatDesVentesArrivee
                var etat = new EtatDesVentesArrivee
                {
                    Numero = vm.Numero,
                    DateEdition = vm.DateEdition,
                    EquipageId = equipage.EquipageId,
                    AgentSaisieId = user.Id,
                    TotalEncaisse = vm.TotalEncaisse,
                    EcartPlus = vm.EcartPlus,
                    EcartMoins = vm.EcartMoins,
                    FeuilleEtatDesVentes = new List<FeuilleEtatDesVentesArrivee>(),
                    ArticleVentes = articleVentes
                };

                foreach (var f in feuilles)
                {
                    var junction = new FeuilleEtatDesVentesArrivee
                    {
                        FeuilleDeLignes = f,
                        EtatDesVentesArrivee = etat
                    };
                    etat.FeuilleEtatDesVentes.Add(junction);
                }

                foreach (var av in articleVentes)
                {
                    av.EtatDesVentesArrivee = etat;
                }

                _context.EtatDesVentesArrivee.Add(etat);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Saved EtatDesVentesArrivee: Numero={etat.Numero}, VolId={vol.Id}");

                // Update Vol to link to EtatDesVentesArrivee
                vol.EtatDesVentesArriveeId = etat.Id;
                _context.Vols.Update(vol);
                await _context.SaveChangesAsync();

                ViewBag.Success = true;
                return PartialView("AddEtatDeVenteArrivee", new EtatDesVentesArriveeViewModel());
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"Database error: {dbEx.InnerException?.Message ?? dbEx.Message}");
                ViewBag.ErrorMessage = $"Erreur lors de l'enregistrement des données: {dbEx.InnerException?.Message ?? dbEx.Message}";
                return PartialView("AddEtatDeVenteArrivee", vm);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error: {ex.Message}");
                ViewBag.ErrorMessage = $"Erreur lors de l'enregistrement de l'état: {ex.Message}";
                return PartialView("AddEtatDeVenteArrivee", vm);
            }
        }

        // GET: DataTable AJAX source
        [HttpGet]
        public IActionResult GetEtatDesVentesArrivee()
        {
            try
            {
                var etats = _context.EtatDesVentesArrivee
                    .Include(e => e.AgentSaisie)
                    .Include(e => e.ArticleVentes) // Include ArticleVentes for totals
                    .Select(e => new
                    {
                        e.Id,
                        numero = e.Numero,
                        dateEdition = e.DateEdition,
                        agentSaisie = e.AgentSaisie != null ? e.AgentSaisie.NomPrenom : "Unknown",
                     
                        totalItemsSold = e.ArticleVentes.Sum(av => av.QuantiteVendu),
                        totalRevenue = e.ArticleVentes.Sum(av => av.QuantiteVendu * av.PrixUnitaireHT),
                        totalEncaisse = e.TotalEncaisse
                    })
                    .ToList();
                return Json(etats);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetEtatDesVentesArrivee Error: {ex.Message}");
                return StatusCode(500, new { error = "Erreur serveur lors de la récupération des états." });
            }
        }
        // GET: DataTable AJAX source
        [HttpGet]
        public IActionResult GetEtatDesVentesDepart()
        {
            try
            {
                var etats = _context.EtatDesVentesDepart
                    .Include(e => e.AgentSaisie)
                    .Include(e => e.ArticleVentesDepart) // Include ArticleVentes for totals
                    .Select(e => new
                    {
                        e.Id,
                        numero = e.Numero,
                        dateEdition = e.DateEdition,
                        agentSaisie = e.AgentSaisie != null ? e.AgentSaisie.NomPrenom : "Unknown",

                        totalItemsSold = e.ArticleVentesDepart.Sum(av => av.QuantiteVendu),
                        totalRevenue = e.ArticleVentesDepart.Sum(av => av.QuantiteVendu * av.PrixUnitaireHT),
                        totalEncaisse = e.TotalEncaisse
                    })
                    .ToList();
                return Json(etats);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetEtatDesVentesArrivee Error: {ex.Message}");
                return StatusCode(500, new { error = "Erreur serveur lors de la récupération des états." });
            }
        }
        [HttpPost]
        public IActionResult DeleteEtatDesVentesDepart(string numero)
        {
            try
            {
                var etat = _context.EtatDesVentesDepart
                    .Include(e => e.ArticleVentesDepart)
                    .FirstOrDefault(e => e.Numero == numero);

                if (etat == null)
                    return NotFound("État des ventes introuvable.");

                // Delete associated articles first
                if (etat.ArticleVentesDepart != null && etat.ArticleVentesDepart.Any())
                {
                    _context.ArticleVenteDepart.RemoveRange(etat.ArticleVentesDepart);
                }

                // Delete the main record
                _context.EtatDesVentesDepart.Remove(etat);
                _context.SaveChanges();

                return Ok("Supprimé avec succès.");
            }
            catch (Exception ex)
            {
                // Optional: log ex
                return BadRequest("Erreur lors de la suppression : " + ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEtatDesVentesArriveeByNumero(string numero)
        {
            try
            {
                var etat = await _context.EtatDesVentesArrivee.FirstOrDefaultAsync(e => e.Numero == numero);
                if (etat == null)
                {
                    return Json(new { success = false, message = "Record not found." });
                }
                _context.EtatDesVentesArrivee.Remove(etat);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (DbUpdateException)
            {
                return Json(new { success = false, message = "Impossible de supprimer car des enregistrements associés existent." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> ViewEtatDesVentesArrivee(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
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
                .Include(e => e.ArticleVentes) // Changed from ArticlesVendus to match your model
                .ThenInclude(a => a.Article)
                .ThenInclude(a => a.Categorie)
                .FirstOrDefaultAsync(e => e.Id == id && e.AgentSaisieId == user.Id);

            if (etat == null)
            {
                return NotFound("État des Ventes non trouvé ou vous n'êtes pas autorisé à le voir.");
            }

            var vol = await _context.Vols
                .FirstOrDefaultAsync(v => v.EtatDesVentesArriveeId == id);

            // Create a list of ArticleVenteViewModel objects that match the structure expected by the view
            var articleVentes = new List<ArticleVenteViewModel>();

            if (etat.ArticleVentes != null)
            {
                foreach (var av in etat.ArticleVentes)
                {
                    articleVentes.Add(new ArticleVenteViewModel
                    {
                        ArticleId = av.ArticleId,
                        IdArticle = av.Article.IdArticle, // Make sure this property exists in your Article model
                        Description = av.Article.Description,
                        CategorieId = av.Article.CategorieId,
                        CategorieDescription = av.Article.Categorie.Description,
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
                Numero = etat.Numero,
                DateEdition = etat.DateEdition,
                AgentSaisieId = etat.AgentSaisieId,
                AgentSaisieName = user.NomPrenom,
                FeuilleDeLignesNumeros = etat.FeuilleEtatDesVentes.Select(f => f.FeuilleDeLignes.Numero).ToList(),
                Cc1Name = $"{etat.Equipage.Cc1.Nom} {etat.Equipage.Cc1.Prenom}",
                Cc1Matricule = etat.Equipage.Cc1.Matricule,
                Cc2Name = etat.Equipage.Cc2 != null ? $"{etat.Equipage.Cc2.Nom} {etat.Equipage.Cc2.Prenom}" : null,
                Cc2Matricule = etat.Equipage.Cc2?.Matricule,
                Pnc1Name = $"{etat.Equipage.Pnc1.Nom} {etat.Equipage.Pnc1.Prenom}",
                Pnc1Matricule = etat.Equipage.Pnc1.Matricule,
                Pnc2Name = etat.Equipage.Pnc2 != null ? $"{etat.Equipage.Pnc2.Nom} {etat.Equipage.Pnc2.Prenom}" : null,
                Pnc2Matricule = etat.Equipage.Pnc2?.Matricule,
                DynamicPncs = etat.Equipage.PncEquipages?.Select(pe => new DynamicPnc
                {
                    Name = $"{pe.Pnc.Nom} {pe.Pnc.Prenom}",
                    Matricule = pe.Pnc.Matricule
                }).ToList() ?? new List<DynamicPnc>(),
                // Vol details
                FlightNumber = vol?.Numero ?? 0,
                FlightDate = vol?.Date ?? DateTime.MinValue,
                Destination = vol?.Destination,
                Aircraft = vol?.Aircraft,
                FlightTime = vol?.Heure ?? TimeSpan.Zero,
                FlightType = vol?.TypeVol,
                TotalPassengers = vol?.NombrePassagers ?? 0,
                // Financial details
                TotalEncaisse = etat.TotalEncaisse,
                EcartPlus = etat.EcartPlus,
                EcartMoins = etat.EcartMoins,
                // Set the ArticleVentes property directly
                ArticleVentes = articleVentes
            };

            return PartialView("ViewEtatDeVenteArrivee", vm);
        }
        public async Task<IActionResult> ValidateFlightNumber(int flightNumber, string feuilleNumeros)
        {
            if (string.IsNullOrEmpty(feuilleNumeros))
            {
                return Json(new { isValid = false, message = "Veuillez spécifier au moins un numéro de Feuille de Lignes." });
            }

            var numeroList = feuilleNumeros.Split(',').Select(n => int.TryParse(n, out int num) ? num : 0).Where(n => n > 0).ToList();
            if (!numeroList.Any())
            {
                return Json(new { isValid = false, message = "Aucun numéro de Feuille de Lignes valide fourni." });
            }

            var feuille = await _context.FeuilleDeLignes
                .Include(f => f.Vols)
                .FirstOrDefaultAsync(f => numeroList.Contains(f.Numero));

            if (feuille == null)
            {
                return Json(new { isValid = false, message = "Aucune Feuille de Lignes correspondante trouvée." });
            }

            var volExists = feuille.Vols.Any(v => v.Numero == flightNumber);
            if (!volExists)
            {
                return Json(new { isValid = false, message = $"Le vol {flightNumber} n'existe pas dans les Feuilles de Lignes spécifiées." });
            }

            return Json(new { isValid = true });
        }
        [HttpGet]
        public async Task<IActionResult> GetArticleDetails(int idArticle)
        {
            var article = await _context.Articles
                .Include(a => a.Categorie)
                .FirstOrDefaultAsync(a => a.IdArticle == idArticle);

            if (article == null)
            {
                return Json(new { success = false, message = $"Article avec ID {idArticle} n'existe pas." });
            }

            return Json(new
            {
                success = true,
                article = new
                {
                    id = article.Id,
                    idArticle = article.IdArticle,
                    description = article.Description,
                    categorieId = article.CategorieId,
                    categorieDescription = article.Categorie.Description ?? "Unknown",
                    prixUnitaireHT = article.PrixUnitaireHT.ToString("F2", CultureInfo.InvariantCulture)
                }
            });
        }
        [HttpGet]
        public async Task<IActionResult> AgentAccountSettings()
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

            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            // Create the model to pass to the view
            var model = new UserManagementViewModel
            {
                NomPrenom = user.NomPrenom,
                Email = user.Email,
                Telephone = user.Telephone,
                Role = roles.FirstOrDefault() ?? "User" // Get the first role or default to 'User'
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
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(UserManagementViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            // Update user properties
            user.NomPrenom = model.NomPrenom;
            user.Email = model.Email;
            user.Telephone = model.Telephone;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user); // Ensure the changes take effect
                TempData["SuccessMessage"] = "Profile updated successfully!";
                return PartialView("AgentAccountSettings", model);
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View("AgentAccountSettings", model);
        }


    }
}