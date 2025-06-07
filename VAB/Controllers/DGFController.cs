using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VAB.Models;
using VAB.ViewModel;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using VAB.Data;

namespace VAB.Controllers
{
    [Authorize(Roles = "DGF")]
    public class DGFController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;

        public DGFController(ApplicationDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<IActionResult> Dashboard(DateTime? dateDebut, DateTime? dateFin)
        {
            var query = _context.EtatDesVentesArrivee
                .Include(e => e.Equipage)
                    .ThenInclude(eq => eq.Cc1)
                .Include(e => e.Equipage)
                    .ThenInclude(eq => eq.Pnc1)
                .Include(e => e.ArticleVentes)
                    .ThenInclude(av => av.Article)
                .Where(e => e.IsValidated);

            if (dateDebut.HasValue)
                query = query.Where(e => e.DateEdition >= dateDebut.Value);

            if (dateFin.HasValue)
                query = query.Where(e => e.DateEdition <= dateFin.Value);

            var etatsValidated = await query.ToListAsync();

            // IMPORTANT: Get EcartMoins data for ViewBag
            var etatsWithEcart = await _context.EtatDesVentesArrivee
                .Where(e => e.IsValidated)
                .Where(e => dateDebut == null || e.DateEdition >= dateDebut.Value)
                .Where(e => dateFin == null || e.DateEdition <= dateFin.Value)
                .Select(e => new {
                    Id = e.Id,
                    EcartMoins = e.EcartMoins ?? 0,
                    TotalEncaisse = e.TotalEncaisse ?? 0
                })
                .ToListAsync();

            ViewBag.EtatsWithEcart = etatsWithEcart;

            var commissionsCalculees = await _context.PncCommissionCalculations
                .Include(c => c.EtatDesVentesArrivee)
                .Where(c => dateDebut == null || c.DateCalcul >= dateDebut.Value)
                .Where(c => dateFin == null || c.DateCalcul <= dateFin.Value)
                .ToListAsync();

            var viewModel = new DGFDashboardViewModel
            {
                DateDebut = dateDebut,
                DateFin = dateFin,
                EtatsValidated = etatsValidated.Select(e => new EtatDesVentesValidatedViewModel
                {
                    Id = e.Id,
                    Numero = e.Numero,
                    DateEdition = e.DateEdition,
                    EquipageInfo = $"CC1: {e.Equipage.Cc1.Nom} {e.Equipage.Cc1.Prenom} - PNC1: {e.Equipage.Pnc1.Nom} {e.Equipage.Pnc1.Prenom}",
                    TotalEncaisse = e.TotalEncaisse ?? 0,
                    CommissionCalculee = commissionsCalculees.Any(c => c.EtatDesVentesArriveeId == e.Id)
                }).ToList(),
                CommissionsCalculees = commissionsCalculees.Select(c => new PncCommissionSummaryViewModel
                {
                    EtatDesVentesArriveeId = c.EtatDesVentesArriveeId,
                    NumeroEtat = c.EtatDesVentesArrivee.Numero,
                    DateCalcul = c.DateCalcul,
                    TotalEncaisse = c.TotalEncaisse,
                    TotalCommissions = c.CommissionTotalPnc1 + (c.CommissionParPnc * (c.NombrePncsTotal - 1)),
                    NombrePncs = c.NombrePncsTotal
                }).ToList(),
                TotalCommissions = commissionsCalculees.Sum(c => c.CommissionTotalPnc1 + (c.CommissionParPnc * (c.NombrePncsTotal - 1)))
            };

            return View(viewModel);
        }

        public async Task<IActionResult> CalculateCommission(int etatId)
        {
            var etat = await _context.EtatDesVentesArrivee
                .Include(e => e.Equipage)
                    .ThenInclude(eq => eq.Cc1)
                .Include(e => e.Equipage)
                    .ThenInclude(eq => eq.Cc2)
                .Include(e => e.Equipage)
                    .ThenInclude(eq => eq.Pnc1)
                .Include(e => e.Equipage)
                    .ThenInclude(eq => eq.Pnc2)
                .Include(e => e.Equipage)
                    .ThenInclude(eq => eq.PncEquipages)
                        .ThenInclude(pe => pe.Pnc)
                .Include(e => e.ArticleVentes)
                    .ThenInclude(av => av.Article)
                .FirstOrDefaultAsync(e => e.Id == etatId);

            if (etat == null || !etat.IsValidated)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "État des ventes non trouvé ou non validé." });
                }
                TempData["Error"] = "État des ventes non trouvé ou non validé.";
                return RedirectToAction(nameof(Dashboard));
            }

            // Check if commission already calculated
            var existingCalculation = await _context.PncCommissionCalculations
                .FirstOrDefaultAsync(c => c.EtatDesVentesArriveeId == etatId);

            if (existingCalculation != null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return await ViewCommission(etatId);
                }
                TempData["Warning"] = "Commission déjà calculée pour cet état.";
                return RedirectToAction(nameof(ViewCommission), new { etatId = etatId });
            }

            // Calculate total encaisse
            var totalEncaisse = etat.ArticleVentes.Sum(av => av.QuantiteVendu * av.PrixUnitaireHT);

            // Get all PNCs in the equipage
            var allPncs = new List<Pnc>();
            allPncs.Add(etat.Equipage.Pnc1);
            if (etat.Equipage.Pnc2 != null)
                allPncs.Add(etat.Equipage.Pnc2);
            allPncs.AddRange(etat.Equipage.PncEquipages.Select(pe => pe.Pnc));

            var nombrePncsTotal = allPncs.Count;

            // Calculate commissions
            var commissionPnc1Individual = totalEncaisse * 0.01m; // 1% for PNC1
            var commissionCollective = totalEncaisse * 0.15m; // 15% total
            var commissionParPnc = commissionCollective / nombrePncsTotal; // Equal share for all
            var commissionTotalPnc1 = commissionPnc1Individual + commissionParPnc;

            var viewModel = new PncCommissionViewModel
            {
                EtatDesVentesArriveeId = etat.Id,
                NumeroEtat = etat.Numero,
                DateEdition = etat.DateEdition,
                EquipageInfo = $"CC1: {etat.Equipage.Cc1.Nom} {etat.Equipage.Cc1.Prenom}",
                TotalEncaisse = totalEncaisse,
                CommissionPnc1Individual = commissionPnc1Individual,
                CommissionCollective = commissionCollective,
                NombrePncsTotal = nombrePncsTotal,
                CommissionParPnc = commissionParPnc,
                PncCommissions = allPncs.Select(pnc => new PncCommissionDetailViewModel
                {
                    PncId = pnc.PncId,
                    NomComplet = $"{pnc.Nom} {pnc.Prenom}",
                    Matricule = pnc.Matricule,
                    IsPnc1 = pnc.PncId == etat.Equipage.Pnc1Id,
                    CommissionIndividuelle = pnc.PncId == etat.Equipage.Pnc1Id ? commissionPnc1Individual : 0,
                    CommissionCollective = commissionParPnc,
                    CommissionTotale = pnc.PncId == etat.Equipage.Pnc1Id ? commissionTotalPnc1 : commissionParPnc
                }).ToList()
            };

            // Return partial view for AJAX requests
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_CalculateCommission", viewModel);
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SaveCommission(PncCommissionViewModel model)
        {
            try
            {
                var calculation = new PncCommissionCalculation
                {
                    EtatDesVentesArriveeId = model.EtatDesVentesArriveeId,
                    DateCalcul = DateTime.Now,
                    TotalEncaisse = model.TotalEncaisse,
                    CommissionPnc1Individual = model.CommissionPnc1Individual,
                    CommissionCollective = model.CommissionCollective,
                    CommissionPnc1FromCollective = model.CommissionParPnc,
                    CommissionTotalPnc1 = model.CommissionPnc1Individual + model.CommissionParPnc,
                    NombrePncsTotal = model.NombrePncsTotal,
                    CommissionParPnc = model.CommissionParPnc,
                    CalculatedBy = User.Identity.Name
                };

                _context.PncCommissionCalculations.Add(calculation);
                await _context.SaveChangesAsync();

                // Save details
                foreach (var pncCommission in model.PncCommissions)
                {
                    var detail = new PncCommissionDetail
                    {
                        PncCommissionCalculationId = calculation.Id,
                        PncId = pncCommission.PncId,
                        IsPnc1 = pncCommission.IsPnc1,
                        CommissionIndividuelle = pncCommission.CommissionIndividuelle,
                        CommissionCollective = pncCommission.CommissionCollective,
                        CommissionTotale = pncCommission.CommissionTotale
                    };

                    _context.PncCommissionDetails.Add(detail);
                }

                await _context.SaveChangesAsync();

                // Return JSON response for AJAX requests
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Commission calculée et sauvegardée avec succès." });
                }

                TempData["Success"] = "Commission calculée et sauvegardée avec succès.";
                return RedirectToAction(nameof(ViewCommission), new { etatId = calculation.EtatDesVentesArriveeId });
            }
            catch (Exception ex)
            {
                // Return JSON response for AJAX requests
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Erreur lors de la sauvegarde: " + ex.Message });
                }

                TempData["Error"] = "Erreur lors de la sauvegarde: " + ex.Message;
                return View("CalculateCommission", model);
            }
        }

        public async Task<IActionResult> ViewCommission(int etatId)
        {
            var calculation = await _context.PncCommissionCalculations
                .Include(c => c.EtatDesVentesArrivee)
                    .ThenInclude(e => e.Equipage)
                        .ThenInclude(eq => eq.Cc1)
                .Include(c => c.PncCommissionDetails)
                    .ThenInclude(d => d.Pnc)
                .FirstOrDefaultAsync(c => c.EtatDesVentesArriveeId == etatId);

            if (calculation == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Calcul de commission non trouvé." });
                }
                TempData["Error"] = "Calcul de commission non trouvé.";
                return RedirectToAction(nameof(Dashboard));
            }

            var viewModel = new PncCommissionViewModel
            {
                EtatDesVentesArriveeId = calculation.EtatDesVentesArriveeId,
                NumeroEtat = calculation.EtatDesVentesArrivee.Numero,
                DateEdition = calculation.EtatDesVentesArrivee.DateEdition,
                EquipageInfo = $"CC1: {calculation.EtatDesVentesArrivee.Equipage.Cc1.Nom} {calculation.EtatDesVentesArrivee.Equipage.Cc1.Prenom}",
                TotalEncaisse = calculation.TotalEncaisse,
                CommissionPnc1Individual = calculation.CommissionPnc1Individual,
                CommissionCollective = calculation.CommissionCollective,
                NombrePncsTotal = calculation.NombrePncsTotal,
                CommissionParPnc = calculation.CommissionParPnc,
                PncCommissions = calculation.PncCommissionDetails.Select(d => new PncCommissionDetailViewModel
                {
                    PncId = d.PncId,
                    NomComplet = $"{d.Pnc.Nom} {d.Pnc.Prenom}",
                    Matricule = d.Pnc.Matricule,
                    IsPnc1 = d.IsPnc1,
                    CommissionIndividuelle = d.CommissionIndividuelle,
                    CommissionCollective = d.CommissionCollective,
                    CommissionTotale = d.CommissionTotale
                }).ToList()
            };

            // Return partial view for AJAX requests
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ViewCommission", viewModel);
            }

            return View(viewModel);
        }

        public IActionResult TauxEchange()
        {
            return View();
        }
        // GET: ExchangeRate/Edit
        public async Task<IActionResult> Edit()
        {
            var exchangeRate = await _context.ExchangeRates.FirstOrDefaultAsync();

            if (exchangeRate == null)
            {
                // If no entry exists, create a default entry
                exchangeRate = new ExchangeRate
                {
                    EuroToDinar = 1,  // Initial rate
                    LastUpdated = DateTime.Now
                };
                _context.ExchangeRates.Add(exchangeRate);
                await _context.SaveChangesAsync();
            }

            // Return the current exchange rate to the view model
            var model = new ExchangeRateViewModel
            {
                EuroToDinar = exchangeRate.EuroToDinar
            };

            // Return the view TauxEchange.cshtml
            return View("TauxEchange", model);
        }

        // POST: ExchangeRate/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ExchangeRateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var exchangeRate = await _context.ExchangeRates.FirstOrDefaultAsync();

                if (exchangeRate != null)
                {
                    exchangeRate.EuroToDinar = model.EuroToDinar;
                    exchangeRate.LastUpdated = DateTime.Now;
                    _context.Update(exchangeRate);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Edit));  // Redirect to the Edit page after successful update
                }
            }

            // If we reach here, something went wrong, return the form with error messages
            return View("TauxEchange", model);
        }


        [HttpGet]
        public async Task<IActionResult> TrouDeCaisseDashboard(DateTime? dateDebut, DateTime? dateFin)
        {
            var query = _context.TrouDeCaisses
                .Include(t => t.EtatDesVentesArrivee)
                .Include(t => t.PncResponsable)
                .AsQueryable();

            if (dateDebut.HasValue)
                query = query.Where(t => t.DateDetection >= dateDebut.Value);

            if (dateFin.HasValue)
                query = query.Where(t => t.DateDetection <= dateFin.Value);

            var trousDeCaisse = await query.ToListAsync();

            var viewModel = new TrouDeCaisseDashboardViewModel
            {
                DateDebut = dateDebut,
                DateFin = dateFin,
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
                NombreTrousEnAttente = trousDeCaisse.Count(t => t.Statut == "En Attente"),
                NombreTrousAppliques = trousDeCaisse.Count(t => t.EstApplique)
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> DetectTrouDeCaisse(int etatId)
        {
            try
            {
                Console.WriteLine($"=== DETECT TROU DE CAISSE START ===");
                Console.WriteLine($"EtatId: {etatId}");

                var etat = await _context.EtatDesVentesArrivee
                    .Include(e => e.Equipage)
                        .ThenInclude(eq => eq.Pnc1)
                    .Include(e => e.Equipage)
                        .ThenInclude(eq => eq.Cc1)
                    .Include(e => e.ArticleVentes)
                        .ThenInclude(av => av.Article)
                    .FirstOrDefaultAsync(e => e.Id == etatId);

                if (etat == null)
                {
                    Console.WriteLine("État not found!");

                    // Return JSON response for AJAX requests
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "État des ventes non trouvé." });
                    }

                    TempData["Error"] = "État des ventes non trouvé.";
                    return RedirectToAction(nameof(Dashboard));
                }

                Console.WriteLine($"Found État: {etat.Numero}");
                Console.WriteLine($"EcartMoins: {etat.EcartMoins}");
                Console.WriteLine($"TotalEncaisse: {etat.TotalEncaisse}");

                // Check if trou de caisse already exists
                var existingTrou = await _context.TrouDeCaisses
                    .FirstOrDefaultAsync(t => t.EtatDesVentesArriveeId == etatId);

                if (existingTrou != null)
                {
                    Console.WriteLine("Trou de caisse already exists, redirecting to view");
                    return RedirectToAction(nameof(ViewTrouDeCaisse), new { id = existingTrou.Id });
                }

                // Calculate amounts
                var montantCalcule = etat.ArticleVentes?.Sum(av => av.QuantiteVendu * av.PrixUnitaireHT) ?? 0;
                var montantEncaisse = etat.TotalEncaisse ?? 0;
                var ecartMoins = etat.EcartMoins ?? 0;

                Console.WriteLine($"Montant Calculé: {montantCalcule:C}");
                Console.WriteLine($"Montant Encaissé: {montantEncaisse:C}");
                Console.WriteLine($"Écart Moins: {ecartMoins:C}");

                // SIMPLE LOGIC: EcartMoins IS the shortage
                var totalManquant = ecartMoins;

                // Also add any difference between calculated and collected
                if (montantCalcule > montantEncaisse)
                {
                    totalManquant += (montantCalcule - montantEncaisse);
                }

                Console.WriteLine($"Total Manquant: {totalManquant:C}");

                if (totalManquant <= 0)
                {
                    Console.WriteLine("No shortage detected");

                    // Return JSON response for AJAX requests
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new
                        {
                            success = false,
                            message = $"Aucun trou de caisse détecté pour l'état {etat.Numero}. EcartMoins = {ecartMoins:C}, Montant calculé = {montantCalcule:C}, Montant encaissé = {montantEncaisse:C}"
                        });
                    }

                    TempData["Info"] = $"Aucun trou de caisse détecté pour l'état {etat.Numero}. EcartMoins = {ecartMoins:C}";
                    return RedirectToAction(nameof(Dashboard));
                }

                var motif = ecartMoins > 0 ? $"Écart négatif de {ecartMoins:C}" : $"Différence calcul/encaissé de {(montantCalcule - montantEncaisse):C}";

                var viewModel = new TrouDeCaisseViewModel
                {
                    EtatDesVentesArriveeId = etat.Id,
                    NumeroEtat = etat.Numero,
                    DateEtat = etat.DateEdition,
                    PncResponsableId = etat.Equipage.Pnc1Id,
                    PncResponsableNom = $"{etat.Equipage.Pnc1.Nom} {etat.Equipage.Pnc1.Prenom}",
                    PncResponsableMatricule = etat.Equipage.Pnc1.Matricule,
                    DateDetection = DateTime.Now,
                    MontantCalcule = montantCalcule,
                    MontantEncaisse = montantEncaisse,
                    MontantManquant = totalManquant,
                    DetectePar = User.Identity.Name,
                    Statut = "En Attente",
                    EquipageInfo = $"CC1: {etat.Equipage.Cc1.Nom} {etat.Equipage.Cc1.Prenom}",
                    Motif = motif,
                    ArticlesVendus = etat.ArticleVentes?
                        .Where(av => av.QuantiteVendu > 0)
                        .Select(av => new ArticleVenteDetailViewModel
                        {
                            Description = av.Article.Description,
                            QuantiteVendu = av.QuantiteVendu,
                            PrixUnitaire = av.PrixUnitaireHT,
                            Valeur = av.QuantiteVendu * av.PrixUnitaireHT
                        }).ToList() ?? new List<ArticleVenteDetailViewModel>()
                };

                Console.WriteLine($"Created ViewModel with MontantManquant: {viewModel.MontantManquant:C}");

                // Return partial view for AJAX requests
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("_DetectTrouDeCaisse", viewModel);
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DetectTrouDeCaisse: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveTrouDeCaisse(TrouDeCaisseViewModel model)
        {
            try
            {
                Console.WriteLine($"=== SAVE TROU DE CAISSE ===");
                Console.WriteLine($"EtatId: {model.EtatDesVentesArriveeId}");
                Console.WriteLine($"MontantManquant: {model.MontantManquant:C}");
                Console.WriteLine($"Motif: {model.Motif}");

                var trouDeCaisse = new TrouDeCaisse
                {
                    EtatDesVentesArriveeId = model.EtatDesVentesArriveeId,
                    PncResponsableId = model.PncResponsableId,
                    DateDetection = model.DateDetection,
                    MontantCalcule = model.MontantCalcule,
                    MontantEncaisse = model.MontantEncaisse,
                    MontantManquant = model.MontantManquant,
                    Motif = model.Motif,
                    DetectePar = User.Identity.Name ?? "System",
                    Statut = "En Attente",
                    Commentaires = model.Commentaires
                };

                _context.TrouDeCaisses.Add(trouDeCaisse);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Trou de caisse saved with ID: {trouDeCaisse.Id}");

                // Return JSON response for AJAX requests
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Trou de caisse enregistré avec succès." });
                }

                TempData["Success"] = "Trou de caisse enregistré avec succès.";
                return RedirectToAction(nameof(TrouDeCaisseDashboard));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving trou de caisse: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                // Return JSON response for AJAX requests
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Erreur lors de l'enregistrement: " + ex.Message });
                }

                TempData["Error"] = "Erreur lors de l'enregistrement: " + ex.Message;
                return View("DetectTrouDeCaisse", model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewTrouDeCaisse(int id)
        {
            var trouDeCaisse = await _context.TrouDeCaisses
                .Include(t => t.EtatDesVentesArrivee)
                    .ThenInclude(e => e.Equipage)
                        .ThenInclude(eq => eq.Cc1)
                .Include(t => t.EtatDesVentesArrivee)
                    .ThenInclude(e => e.ArticleVentes)
                        .ThenInclude(av => av.Article)
                .Include(t => t.PncResponsable)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trouDeCaisse == null)
            {
                TempData["Error"] = "Trou de caisse non trouvé.";
                return RedirectToAction(nameof(TrouDeCaisseDashboard));
            }

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
                EquipageInfo = $"CC1: {trouDeCaisse.EtatDesVentesArrivee.Equipage.Cc1.Nom} {trouDeCaisse.EtatDesVentesArrivee.Equipage.Cc1.Prenom}",
                ArticlesVendus = trouDeCaisse.EtatDesVentesArrivee.ArticleVentes?
                    .Where(av => av.QuantiteVendu > 0)
                    .Select(av => new ArticleVenteDetailViewModel
                    {
                        Description = av.Article.Description,
                        QuantiteVendu = av.QuantiteVendu,
                        PrixUnitaire = av.PrixUnitaireHT,
                        Valeur = av.QuantiteVendu * av.PrixUnitaireHT
                    }).ToList() ?? new List<ArticleVenteDetailViewModel>()
            };

            // Return partial view for AJAX requests
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ViewTrouDeCaisse", viewModel);
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AppliquerTrouDeCaisse(int id)
        {
            try
            {
                var trouDeCaisse = await _context.TrouDeCaisses
                    .Include(t => t.EtatDesVentesArrivee)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (trouDeCaisse == null)
                {
                    return Json(new { success = false, message = "Trou de caisse non trouvé." });
                }

                if (trouDeCaisse.EstApplique)
                {
                    return Json(new { success = false, message = "Ce trou de caisse a déjà été appliqué." });
                }

                // Check if commission calculation exists
                var commissionCalculation = await _context.PncCommissionCalculations
                    .FirstOrDefaultAsync(c => c.EtatDesVentesArriveeId == trouDeCaisse.EtatDesVentesArriveeId);

                if (commissionCalculation == null)
                {
                    return Json(new { success = false, message = "Aucun calcul de commission trouvé pour cet état des ventes." });
                }

                // Apply the shortage to PNC1's commission
                commissionCalculation.CommissionTotalPnc1 -= trouDeCaisse.MontantManquant;

                // Update trou de caisse status
                trouDeCaisse.EstApplique = true;
                trouDeCaisse.DateApplication = DateTime.Now;
                trouDeCaisse.Statut = "Appliqué";

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Trou de caisse appliqué avec succès à la commission." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erreur lors de l'application: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DetectAllTrousDeCaisse()
        {
            try
            {
                Console.WriteLine("=== AUTO DETECTION START ===");

                // IMPORTANT: Get ALL validated états, regardless of whether they have ArticleVentes
                var etatsValidated = await _context.EtatDesVentesArrivee
                    .Include(e => e.Equipage)
                        .ThenInclude(eq => eq.Pnc1)
                    .Include(e => e.ArticleVentes)
                        .ThenInclude(av => av.Article)
                    .Where(e => e.IsValidated)
                    .ToListAsync();

                Console.WriteLine($"Found {etatsValidated.Count} validated états");

                // Get existing trous de caisse to avoid duplicates
                var existingTrousIds = await _context.TrouDeCaisses
                    .Select(t => t.EtatDesVentesArriveeId)
                    .ToListAsync();

                Console.WriteLine($"Found {existingTrousIds.Count} existing trous de caisse");

                int detectedCount = 0;

                foreach (var etat in etatsValidated)
                {
                    Console.WriteLine($"\n=== Checking État {etat.Numero} (ID: {etat.Id}) ===");
                    Console.WriteLine($"  EcartMoins: {etat.EcartMoins ?? 0}");
                    Console.WriteLine($"  TotalEncaisse: {etat.TotalEncaisse ?? 0}");
                    Console.WriteLine($"  IsValidated: {etat.IsValidated}");

                    // Skip if trou de caisse already exists
                    if (existingTrousIds.Contains(etat.Id))
                    {
                        Console.WriteLine($"  -> Already has trou de caisse, skipping");
                        continue;
                    }

                    // Calculate amounts
                    var montantCalcule = etat.ArticleVentes?.Sum(av => av.QuantiteVendu * av.PrixUnitaireHT) ?? 0;
                    var montantEncaisse = etat.TotalEncaisse ?? 0;
                    var ecartMoins = etat.EcartMoins ?? 0;

                    Console.WriteLine($"  Montant Calculé: {montantCalcule:C}");
                    Console.WriteLine($"  Montant Encaissé: {montantEncaisse:C}");
                    Console.WriteLine($"  Écart Moins: {ecartMoins:C}");

                    // CRITICAL FIX: DIRECTLY USE EcartMoins as the shortage amount
                    // This is the key fix - we're directly using EcartMoins without additional conditions
                    var totalManquant = ecartMoins;

                    // Also check if calculated > collected (additional shortage)
                    if (montantCalcule > montantEncaisse)
                    {
                        var difference = montantCalcule - montantEncaisse;
                        totalManquant += difference;
                        Console.WriteLine($"  Additional shortage (calc > collected): {difference:C}");
                    }

                    Console.WriteLine($"  TOTAL MANQUANT: {totalManquant:C}");

                    // Create trou de caisse if there's any shortage
                    if (totalManquant > 0)
                    {
                        // Make sure we have a valid PNC1 ID
                        if (etat.Equipage?.Pnc1Id == null)
                        {
                            Console.WriteLine($"  -> ⚠️ No PNC1 found for this état, skipping");
                            continue;
                        }

                        var motif = "";
                        if (ecartMoins > 0 && (montantCalcule > montantEncaisse))
                        {
                            motif = $"Écart négatif de {ecartMoins:C} + Différence calcul/encaissé de {(montantCalcule - montantEncaisse):C}";
                        }
                        else if (ecartMoins > 0)
                        {
                            motif = $"Écart négatif de {ecartMoins:C}";
                        }
                        else
                        {
                            motif = $"Différence calcul/encaissé de {(montantCalcule - montantEncaisse):C}";
                        }

                        var trouDeCaisse = new TrouDeCaisse
                        {
                            EtatDesVentesArriveeId = etat.Id,
                            PncResponsableId = etat.Equipage.Pnc1Id,
                            DateDetection = DateTime.Now,
                            MontantCalcule = montantCalcule,
                            MontantEncaisse = montantEncaisse,
                            MontantManquant = totalManquant,
                            Motif = motif,
                            DetectePar = "System Auto-Detection",
                            Statut = "En Attente",
                            Commentaires = "Détection automatique"
                        };

                        _context.TrouDeCaisses.Add(trouDeCaisse);
                        detectedCount++;

                        Console.WriteLine($"  -> ✅ DETECTED! Creating trou de caisse: {totalManquant:C}");
                        Console.WriteLine($"     Motif: {motif}");
                    }
                    else
                    {
                        Console.WriteLine($"  -> ❌ No shortage detected");
                    }
                }

                if (detectedCount > 0)
                {
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"\n=== ✅ DETECTION COMPLETE: {detectedCount} trous detected and saved ===");
                }
                else
                {
                    Console.WriteLine($"\n=== ℹ️ DETECTION COMPLETE: No new trous detected ===");
                }

                return Json(new
                {
                    success = true,
                    message = $"{detectedCount} trou(s) de caisse détecté(s) automatiquement.",
                    details = $"Processed {etatsValidated.Count} validated états, found {detectedCount} new shortages."
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in auto detection: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new { success = false, message = "Erreur lors de la détection automatique: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DebugEcartMoins()
        {
            try
            {
                Console.WriteLine("=== DEBUG ECART MOINS START ===");

                // Get ALL états with EcartMoins > 0, regardless of validation status
                var etatsWithEcartMoins = await _context.EtatDesVentesArrivee
                    .Include(e => e.Equipage)
                        .ThenInclude(eq => eq.Pnc1)
                    .Where(e => e.EcartMoins.HasValue && e.EcartMoins > 0)
                    .ToListAsync();

                Console.WriteLine($"Found {etatsWithEcartMoins.Count} états with EcartMoins > 0");

                // Get existing trous de caisse
                var existingTrous = await _context.TrouDeCaisses
                    .Select(t => t.EtatDesVentesArriveeId)
                    .ToListAsync();

                var result = etatsWithEcartMoins.Select(e => new {
                    Id = e.Id,
                    Numero = e.Numero,
                    EcartMoins = e.EcartMoins,
                    TotalEncaisse = e.TotalEncaisse,
                    IsValidated = e.IsValidated,
                    PncResponsable = e.Equipage?.Pnc1 != null ? $"{e.Equipage.Pnc1.Nom} {e.Equipage.Pnc1.Prenom}" : "No PNC1",
                    HasTrouDeCaisse = existingTrous.Contains(e.Id),
                    DateEdition = e.DateEdition
                }).ToList();

                foreach (var item in result)
                {
                    Console.WriteLine($"État {item.Numero}: EcartMoins={item.EcartMoins}, HasTrou={item.HasTrouDeCaisse}, Validated={item.IsValidated}");
                }

                return Json(new
                {
                    success = true,
                    count = etatsWithEcartMoins.Count,
                    etats = result,
                    message = $"Found {etatsWithEcartMoins.Count} états with EcartMoins > 0",
                    existingTrousCount = existingTrous.Count
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in debug: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Direct creation of trou de caisse for a specific état
        [HttpPost]
        public async Task<IActionResult> CreateTrouDeCaisseForEtat(int etatId)
        {
            try
            {
                Console.WriteLine($"=== DIRECT CREATION FOR ETAT {etatId} ===");

                // Check if trou already exists
                var existingTrou = await _context.TrouDeCaisses
                    .FirstOrDefaultAsync(t => t.EtatDesVentesArriveeId == etatId);

                if (existingTrou != null)
                {
                    return Json(new
                    {
                        success = false,
                        message = $"Un trou de caisse existe déjà pour cet état (ID: {existingTrou.Id})"
                    });
                }

                // Get the état
                var etat = await _context.EtatDesVentesArrivee
                    .Include(e => e.Equipage)
                        .ThenInclude(eq => eq.Pnc1)
                    .Include(e => e.ArticleVentes)
                        .ThenInclude(av => av.Article)
                    .FirstOrDefaultAsync(e => e.Id == etatId);

                if (etat == null)
                {
                    return Json(new { success = false, message = "État des ventes non trouvé." });
                }

                if (etat.Equipage?.Pnc1Id == null)
                {
                    return Json(new { success = false, message = "Pas de PNC1 trouvé pour cet état." });
                }

                // Calculate amounts
                var montantCalcule = etat.ArticleVentes?.Sum(av => av.QuantiteVendu * av.PrixUnitaireHT) ?? 0;
                var montantEncaisse = etat.TotalEncaisse ?? 0;
                var ecartMoins = etat.EcartMoins ?? 0;

                // DIRECTLY use EcartMoins as the shortage
                var totalManquant = ecartMoins;

                // Also add difference if calculated > collected
                if (montantCalcule > montantEncaisse)
                {
                    totalManquant += (montantCalcule - montantEncaisse);
                }

                if (totalManquant <= 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Pas de manque détecté pour cet état."
                    });
                }

                var motif = ecartMoins > 0 ? $"Écart négatif de {ecartMoins:C}" : $"Différence calcul/encaissé de {(montantCalcule - montantEncaisse):C}";

                var trouDeCaisse = new TrouDeCaisse
                {
                    EtatDesVentesArriveeId = etat.Id,
                    PncResponsableId = etat.Equipage.Pnc1Id,
                    DateDetection = DateTime.Now,
                    MontantCalcule = montantCalcule,
                    MontantEncaisse = montantEncaisse,
                    MontantManquant = totalManquant,
                    Motif = motif,
                    DetectePar = User.Identity.Name ?? "Manual Creation",
                    Statut = "En Attente",
                    Commentaires = "Création manuelle directe"
                };

                _context.TrouDeCaisses.Add(trouDeCaisse);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = $"Trou de caisse créé avec succès pour l'état {etat.Numero}. Montant: {totalManquant:C}",
                    trouId = trouDeCaisse.Id
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating trou de caisse: {ex.Message}");
                return Json(new { success = false, message = $"Erreur: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> PncBilan(int pncId, DateTime? dateDebut, DateTime? dateFin)
        {
            // Fetch PNC details
            var pnc = await _context.Pncs
                .FirstOrDefaultAsync(p => p.PncId == pncId);

            if (pnc == null)
            {
                TempData["Error"] = "PNC non trouvé.";
                return RedirectToAction(nameof(PncBilanDashboard));
            }

            // Fetch commissions (credits)
            var commissionQuery = _context.PncCommissionDetails
                .Include(d => d.PncCommissionCalculation)
                    .ThenInclude(c => c.EtatDesVentesArrivee)
                .Where(d => d.PncId == pncId);

            // Fetch trous de caisse (debits)
            var trouQuery = _context.TrouDeCaisses
                .Include(t => t.EtatDesVentesArrivee)
                .Where(t => t.PncResponsableId == pncId && t.EstApplique);

            // Apply date filters
            if (dateDebut.HasValue)
            {
                commissionQuery = commissionQuery.Where(d => d.PncCommissionCalculation.DateCalcul >= dateDebut.Value);
                trouQuery = trouQuery.Where(t => t.DateApplication >= dateDebut.Value);
            }
            if (dateFin.HasValue)
            {
                commissionQuery = commissionQuery.Where(d => d.PncCommissionCalculation.DateCalcul <= dateFin.Value);
                trouQuery = trouQuery.Where(t => t.DateApplication <= dateFin.Value);
            }

            var commissions = await commissionQuery.ToListAsync();
            var trous = await trouQuery.ToListAsync();

            // Combine transactions
            var transactions = new List<PncTransactionViewModel>();

            // Add commission transactions (credits)
            foreach (var commission in commissions)
            {
                transactions.Add(new PncTransactionViewModel
                {
                    Id = commission.Id,
                    Date = commission.PncCommissionCalculation.DateCalcul,
                    TypeTransaction = "Commission",
                    Description = $"Commission pour l'état {commission.PncCommissionCalculation.EtatDesVentesArrivee.Numero}",
                    Reference = commission.PncCommissionCalculation.EtatDesVentesArrivee.Numero,
                    Credit = commission.CommissionTotale,
                    Debit = null,
                    Statut = "Validé",
                    CreePar = commission.PncCommissionCalculation.CalculatedBy,
                    EtatDesVentesArriveeId = commission.PncCommissionCalculation.EtatDesVentesArriveeId,
                    CommissionCalculationId = commission.PncCommissionCalculationId,
                    Commentaires = "Commission calculée automatiquement"
                });
            }

            // Add trou de caisse transactions (debits)
            foreach (var trou in trous)
            {
                transactions.Add(new PncTransactionViewModel
                {
                    Id = trou.Id,
                    Date = trou.DateApplication ?? trou.DateDetection,
                    TypeTransaction = "Trou de Caisse",
                    Description = $"Manque détecté pour l'état {trou.EtatDesVentesArrivee.Numero}",
                    Reference = trou.EtatDesVentesArrivee.Numero,
                    Debit = trou.MontantManquant,
                    Credit = null,
                    Statut = trou.Statut,
                    CreePar = trou.DetectePar,
                    EtatDesVentesArriveeId = trou.EtatDesVentesArriveeId,
                    TrouDeCaisseId = trou.Id,
                    Commentaires = trou.Commentaires
                });
            }

            // Sort transactions by date
            transactions = transactions.OrderBy(t => t.Date).ToList();

            // Calculate balances
            decimal runningBalance = 0; // Assuming no initial balance stored
            foreach (var transaction in transactions)
            {
                runningBalance += transaction.Credit ?? 0;
                runningBalance -= transaction.Debit ?? 0;
                transaction.SoldeApres = runningBalance;
            }

            var viewModel = new PncBilanViewModel
            {
                PncId = pnc.PncId,
                PncNom = pnc.Nom,
                PncPrenom = pnc.Prenom,
                PncMatricule = pnc.Matricule,
                DateDebut = dateDebut,
                DateFin = dateFin,
                Transactions = transactions,
                SoldeInitial = 0, // Adjust if you have a mechanism to store initial balance
                TotalCredits = transactions.Sum(t => t.Credit ?? 0),
                TotalDebits = transactions.Sum(t => t.Debit ?? 0),
                SoldeFinal = runningBalance,
                NombreTransactions = transactions.Count
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> PncBilanDashboard(DateTime? dateDebut, DateTime? dateFin)
        {
            // Fetch all PNCs with transactions
            var pncIdsWithCommissions = await _context.PncCommissionDetails
                .Select(d => d.PncId)
                .Distinct()
                .ToListAsync();

            var pncIdsWithTrous = await _context.TrouDeCaisses
                .Where(t => t.EstApplique)
                .Select(t => t.PncResponsableId)
                .Distinct()
                .ToListAsync();

            var allPncIds = pncIdsWithCommissions.Union(pncIdsWithTrous).Distinct().ToList();

            var summaries = new List<PncBilanSummaryViewModel>();

            foreach (var pncId in allPncIds)
            {
                var pnc = await _context.Pncs
                    .FirstOrDefaultAsync(p => p.PncId == pncId);

                if (pnc == null) continue;

                // Fetch commissions
                var commissionQuery = _context.PncCommissionDetails
                    .Include(d => d.PncCommissionCalculation)
                    .Where(d => d.PncId == pncId);

                // Fetch trous de caisse
                var trouQuery = _context.TrouDeCaisses
                    .Where(t => t.PncResponsableId == pncId && t.EstApplique);

                // Apply date filters
                if (dateDebut.HasValue)
                {
                    commissionQuery = commissionQuery.Where(d => d.PncCommissionCalculation.DateCalcul >= dateDebut.Value);
                    trouQuery = trouQuery.Where(t => t.DateApplication >= dateDebut.Value);
                }
                if (dateFin.HasValue)
                {
                    commissionQuery = commissionQuery.Where(d => d.PncCommissionCalculation.DateCalcul <= dateFin.Value);
                    trouQuery = trouQuery.Where(t => t.DateApplication <= dateFin.Value);
                }

                var commissions = await commissionQuery.ToListAsync();
                var trous = await trouQuery.ToListAsync();

                // Combine transactions
                var transactions = new List<PncTransactionViewModel>();

                foreach (var commission in commissions)
                {
                    transactions.Add(new PncTransactionViewModel
                    {
                        Date = commission.PncCommissionCalculation.DateCalcul,
                        Credit = commission.CommissionTotale,
                        Debit = null
                    });
                }

                foreach (var trou in trous)
                {
                    transactions.Add(new PncTransactionViewModel
                    {
                        Date = trou.DateApplication ?? trou.DateDetection,
                        Debit = trou.MontantManquant,
                        Credit = null
                    });
                }

                // Calculate summary
                var totalCredits = transactions.Sum(t => t.Credit ?? 0);
                var totalDebits = transactions.Sum(t => t.Debit ?? 0);
                var lastTransaction = transactions.OrderByDescending(t => t.Date).FirstOrDefault()?.Date;

                summaries.Add(new PncBilanSummaryViewModel
                {
                    PncId = pnc.PncId,
                    PncNomComplet = $"{pnc.Nom} {pnc.Prenom}",
                    PncMatricule = pnc.Matricule,
                    TotalCredits = totalCredits,
                    TotalDebits = totalDebits,
                    SoldeFinal = totalCredits - totalDebits,
                    NombreTransactions = transactions.Count,
                    DerniereTransaction = lastTransaction
                });
            }

            var viewModel = new PncBilanDashboardViewModel
            {
                DateDebut = dateDebut,
                DateFin = dateFin,
                PncSummaries = summaries.OrderBy(s => s.PncNomComplet).ToList(),
                TotalGeneralCredits = summaries.Sum(s => s.TotalCredits),
                TotalGeneralDebits = summaries.Sum(s => s.TotalDebits),
                SoldeGeneral = summaries.Sum(s => s.SoldeFinal)
            };

            return View(viewModel);
        }

    }
}
