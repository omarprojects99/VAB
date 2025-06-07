using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VAB.Data;
using VAB.Models;
using VAB.ViewModel;

namespace VAB.Controllers
{
    public class TrouDeCaisseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrouDeCaisseController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Dashboard - Overview of all cash shortages
        public async Task<IActionResult> Index(DateTime? dateDebut, DateTime? dateFin)
        {
            var query = _context.TrouDeCaisses
                .Include(t => t.EtatDesVentesArrivee)
                .Include(t => t.PncResponsable)
                .AsQueryable();

            if (dateDebut.HasValue)
                query = query.Where(t => t.DateDetection >= dateDebut.Value);

            if (dateFin.HasValue)
                query = query.Where(t => t.DateDetection <= dateFin.Value);

            var trousDeCaisse = await query
                .OrderByDescending(t => t.DateDetection)
                .ToListAsync();

            var viewModel = new TrouDeCaisseDashboardViewModel
            {
                TrousDeCaisse = trousDeCaisse.Select(t => new TrouDeCaisseSummaryViewModel
                {
                    Id = t.Id,
                    NumeroEtat = t.EtatDesVentesArrivee.Numero,
                    DateDetection = t.DateDetection,
                    PncResponsableNom = $"{t.PncResponsable.Nom} {t.PncResponsable.Prenom}",
                    MontantManquant = t.MontantManquant,
                    Statut = t.Statut,
                    EstApplique = t.EstApplique
                }).ToList(),
                TotalManques = trousDeCaisse.Sum(t => t.MontantManquant),
                DateDebut = dateDebut,
                DateFin = dateFin,
                NombreTrousEnAttente = trousDeCaisse.Count(t => t.Statut == "En Attente"),
                NombreTrousAppliques = trousDeCaisse.Count(t => t.EstApplique)
            };

            return View(viewModel);
        }

        // Detect cash shortages for all unprocessed EtatDesVentesArrivee
        [HttpPost]
        public async Task<IActionResult> DetectAllShortages()
        {
            try
            {
                var detectedCount = await DetectAndCreateShortages();

                return Json(new
                {
                    success = true,
                    message = $"{detectedCount} trous de caisse détectés et enregistrés.",
                    count = detectedCount
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Erreur lors de la détection: {ex.Message}"
                });
            }
        }

        // Detect shortages for a specific EtatDesVentesArrivee
        [HttpPost]
        public async Task<IActionResult> DetectShortageForEtat(int etatId)
        {
            try
            {
                var shortage = await DetectShortageForSpecificEtat(etatId);

                if (shortage != null)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Trou de caisse détecté et enregistré.",
                        shortage = new
                        {
                            id = shortage.Id,
                            montantManquant = shortage.MontantManquant,
                            pncResponsable = $"{shortage.PncResponsable.Nom} {shortage.PncResponsable.Prenom}"
                        }
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = true,
                        message = "Aucun trou de caisse détecté pour cet état."
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Erreur: {ex.Message}"
                });
            }
        }

        // View details of a specific cash shortage
        public async Task<IActionResult> Details(int id)
        {
            var trouDeCaisse = await _context.TrouDeCaisses
                .Include(t => t.EtatDesVentesArrivee)
                    .ThenInclude(e => e.ArticleVentes)
                        .ThenInclude(av => av.Article)
                .Include(t => t.PncResponsable)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trouDeCaisse == null)
                return NotFound();

            var viewModel = new TrouDeCaisseViewModel
            {
                Id = trouDeCaisse.Id,
                EtatDesVentesArriveeId = trouDeCaisse.EtatDesVentesArriveeId,
                NumeroEtat = trouDeCaisse.EtatDesVentesArrivee.Numero,
                DateEtat = trouDeCaisse.EtatDesVentesArrivee.DateEdition,
                PncResponsableId = trouDeCaisse.PncResponsableId,
                PncResponsableNom = $"{trouDeCaisse.PncResponsable.Nom} {trouDeCaisse.PncResponsable.Prenom}",
                PncResponsableMatricule = trouDeCaisse.PncResponsable.Matricule,
                DateDetection = trouDeCaisse.DateDetection,
                MontantCalcule = trouDeCaisse.MontantCalcule,
                MontantEncaisse = trouDeCaisse.MontantEncaisse,
                MontantManquant = trouDeCaisse.MontantManquant,
                Motif = trouDeCaisse.Motif,
                Statut = trouDeCaisse.Statut,
                EstApplique = trouDeCaisse.EstApplique,
                DateApplication = trouDeCaisse.DateApplication,
                DetectePar = trouDeCaisse.DetectePar,
                Commentaires = trouDeCaisse.Commentaires,
                ArticlesVendus = trouDeCaisse.EtatDesVentesArrivee.ArticleVentes
                    .Where(av => av.QuantiteVendu > 0)
                    .Select(av => new ArticleVenteDetailViewModel
                    {
                        Description = av.Article.Description,
                        QuantiteVendu = av.QuantiteVendu,
                        PrixUnitaire = av.PrixUnitaireHT,
                        Valeur = av.QuantiteVendu * av.PrixUnitaireHT
                    }).ToList()
            };

            return View(viewModel);
        }

        // Apply shortage to commission calculation
        [HttpPost]
        public async Task<IActionResult> ApplyToCommission(int id)
        {
            try
            {
                var trouDeCaisse = await _context.TrouDeCaisses.FindAsync(id);
                if (trouDeCaisse == null)
                    return Json(new { success = false, message = "Trou de caisse introuvable." });

                trouDeCaisse.EstApplique = true;
                trouDeCaisse.DateApplication = DateTime.Now;
                trouDeCaisse.Statut = "Appliqué";

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Trou de caisse appliqué à la commission avec succès."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Erreur: {ex.Message}"
                });
            }
        }

        // Update shortage status
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status, string commentaires)
        {
            try
            {
                var trouDeCaisse = await _context.TrouDeCaisses.FindAsync(id);
                if (trouDeCaisse == null)
                    return Json(new { success = false, message = "Trou de caisse introuvable." });

                trouDeCaisse.Statut = status;
                trouDeCaisse.Commentaires = commentaires;

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Statut mis à jour avec succès."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Erreur: {ex.Message}"
                });
            }
        }

        // Private method to detect and create shortages
        private async Task<int> DetectAndCreateShortages()
        {
            var etatsWithShortages = await _context.EtatDesVentesArrivee
                .Include(e => e.ArticleVentes)
                .Include(e => e.Equipage)
                    .ThenInclude(eq => eq.Pnc1)
                .Where(e => e.IsValidated &&
                           e.TotalEncaisse.HasValue &&
                           !_context.TrouDeCaisses.Any(t => t.EtatDesVentesArriveeId == e.Id))
                .ToListAsync();

            int detectedCount = 0;

            foreach (var etat in etatsWithShortages)
            {
                var calculatedTotal = etat.ArticleVentes.Sum(av => av.QuantiteVendu * av.PrixUnitaireHT);
                var actualTotal = etat.TotalEncaisse.Value;
                var shortage = calculatedTotal - actualTotal;

                if (shortage > 0) // There's a shortage
                {
                    var trouDeCaisse = new TrouDeCaisse
                    {
                        EtatDesVentesArriveeId = etat.Id,
                        PncResponsableId = etat.Equipage.Pnc1Id,
                        DateDetection = DateTime.Now,
                        MontantCalcule = calculatedTotal,
                        MontantEncaisse = actualTotal,
                        MontantManquant = shortage,
                        DetectePar = User.Identity.Name ?? "System",
                        Statut = "En Attente",
                        EstApplique = false
                    };

                    _context.TrouDeCaisses.Add(trouDeCaisse);
                    detectedCount++;

                    // Update EcartMoins in EtatDesVentesArrivee if not already set
                    if (!etat.EcartMoins.HasValue || etat.EcartMoins.Value != shortage)
                    {
                        etat.EcartMoins = shortage;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return detectedCount;
        }

        // Private method to detect shortage for specific etat
        private async Task<TrouDeCaisse> DetectShortageForSpecificEtat(int etatId)
        {
            var etat = await _context.EtatDesVentesArrivee
                .Include(e => e.ArticleVentes)
                .Include(e => e.Equipage)
                    .ThenInclude(eq => eq.Pnc1)
                .FirstOrDefaultAsync(e => e.Id == etatId);

            if (etat == null || !etat.IsValidated || !etat.TotalEncaisse.HasValue)
                return null;

            // Check if already processed
            var existingTrou = await _context.TrouDeCaisses
                .FirstOrDefaultAsync(t => t.EtatDesVentesArriveeId == etatId);

            if (existingTrou != null)
                return existingTrou;

            var calculatedTotal = etat.ArticleVentes.Sum(av => av.QuantiteVendu * av.PrixUnitaireHT);
            var actualTotal = etat.TotalEncaisse.Value;
            var shortage = calculatedTotal - actualTotal;

            if (shortage > 0) // There's a shortage
            {
                var trouDeCaisse = new TrouDeCaisse
                {
                    EtatDesVentesArriveeId = etat.Id,
                    PncResponsableId = etat.Equipage.Pnc1Id,
                    DateDetection = DateTime.Now,
                    MontantCalcule = calculatedTotal,
                    MontantEncaisse = actualTotal,
                    MontantManquant = shortage,
                    DetectePar = User.Identity.Name ?? "System",
                    Statut = "En Attente",
                    EstApplique = false
                };

                _context.TrouDeCaisses.Add(trouDeCaisse);

                // Update EcartMoins in EtatDesVentesArrivee
                etat.EcartMoins = shortage;

                await _context.SaveChangesAsync();

                // Load the PncResponsable for return
                await _context.Entry(trouDeCaisse)
                    .Reference(t => t.PncResponsable)
                    .LoadAsync();

                return trouDeCaisse;
            }

            return null;
        }

        // Generate report
        public async Task<IActionResult> GenerateReport(DateTime? dateDebut, DateTime? dateFin, string format = "html")
        {
            var query = _context.TrouDeCaisses
                .Include(t => t.EtatDesVentesArrivee)
                .Include(t => t.PncResponsable)
                .AsQueryable();

            if (dateDebut.HasValue)
                query = query.Where(t => t.DateDetection >= dateDebut.Value);

            if (dateFin.HasValue)
                query = query.Where(t => t.DateDetection <= dateFin.Value);

            var trousDeCaisse = await query
                .OrderByDescending(t => t.DateDetection)
                .ToListAsync();

            var viewModel = new TrouDeCaisseDashboardViewModel
            {
                TrousDeCaisse = trousDeCaisse.Select(t => new TrouDeCaisseSummaryViewModel
                {
                    Id = t.Id,
                    NumeroEtat = t.EtatDesVentesArrivee.Numero,
                    DateDetection = t.DateDetection,
                    PncResponsableNom = $"{t.PncResponsable.Nom} {t.PncResponsable.Prenom}",
                    MontantManquant = t.MontantManquant,
                    Statut = t.Statut,
                    EstApplique = t.EstApplique
                }).ToList(),
                TotalManques = trousDeCaisse.Sum(t => t.MontantManquant),
                DateDebut = dateDebut,
                DateFin = dateFin
            };

            if (format.ToLower() == "pdf")
            {
                // Return PDF view
                return View("ReportPdf", viewModel);
            }

            return View("Report", viewModel);
        }
    }
}
