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
    [Authorize]
    public class MotifController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<MotifController> _logger;

        public MotifController(ApplicationDbContext context, UserManager<User> userManager, ILogger<MotifController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Motif (For ResponsableCatering)
        [Authorize(Roles = "ResponsableCatering")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var motifs = await _context.Motifs
                    .OrderByDescending(m => m.DateCreation)
                    .Select(m => new MotifViewModel
                    {
                        Id = m.Id,
                        NumeroEtatVenteArrivee = m.NumeroEtatVenteArrivee,
                        Description = m.Description,
                        DateCreation = m.DateCreation,
                        CreatedBy = m.CreatedBy,
                        Status = m.Status,
                        DateLecture = m.DateLecture,
                        ReadBy = m.ReadBy
                    })
                    .ToListAsync();

                return View(motifs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading motifs");
                TempData["Error"] = "Erreur lors du chargement des motifs.";
                return View(new List<MotifViewModel>());
            }
        }

        // GET: Motif/AgentView (For AgentSaisie)
        [Authorize(Roles = "AgentSaisie")]
        public async Task<IActionResult> AgentView()
        {
            try
            {
                var motifs = await _context.Motifs
                    .OrderByDescending(m => m.DateCreation)
                    .Select(m => new MotifViewModel
                    {
                        Id = m.Id,
                        NumeroEtatVenteArrivee = m.NumeroEtatVenteArrivee,
                        Description = m.Description,
                        DateCreation = m.DateCreation,
                        CreatedBy = m.CreatedBy,
                        Status = m.Status,
                        DateLecture = m.DateLecture,
                        ReadBy = m.ReadBy
                    })
                    .ToListAsync();

                return View(motifs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading motifs for agent");
                TempData["Error"] = "Erreur lors du chargement des motifs.";
                return View(new List<MotifViewModel>());
            }
        }

        // GET: Motif/Create
        [Authorize(Roles = "ResponsableCatering")]
        public IActionResult Create()
        {
            return PartialView("_CreateMotif", new CreateMotifViewModel());
        }

        // POST: Motif/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ResponsableCatering")]
        public async Task<IActionResult> Create(CreateMotifViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_CreateMotif", model);
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    ModelState.AddModelError("", "Utilisateur non connecté.");
                    return PartialView("_CreateMotif", model);
                }

                // Verify that the EtatDesVentesArrivee exists
                var etatExists = await _context.EtatDesVentesArrivee
                    .AnyAsync(e => e.Numero == model.NumeroEtatVenteArrivee);

                if (!etatExists)
                {
                    ModelState.AddModelError("NumeroEtatVenteArrivee",
                        "L'état de vente avec ce numéro n'existe pas.");
                    return PartialView("_CreateMotif", model);
                }

                var motif = new Motif
                {
                    NumeroEtatVenteArrivee = model.NumeroEtatVenteArrivee,
                    Description = model.Description,
                    DateCreation = DateTime.Now,
                    CreatedBy = user.NomPrenom ?? user.UserName ?? "Unknown",
                    Status = MotifStatus.NonLu,
                    DateLecture = null, // Explicitly set to null
                    ReadBy = null // Explicitly set to null
                };

                _context.Motifs.Add(motif);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Motif created successfully by {User} for EtatVente {Numero}",
                    user.UserName, model.NumeroEtatVenteArrivee);

                return Json(new { success = true, message = "Motif créé avec succès." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating motif");
                ModelState.AddModelError("", "Erreur lors de la création du motif.");
                return PartialView("_CreateMotif", model);
            }
        }

        // POST: Motif/MarkAsRead
        [HttpPost]
        [Authorize(Roles = "AgentSaisie")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                var motif = await _context.Motifs.FindAsync(id);
                if (motif == null)
                {
                    return Json(new { success = false, message = "Motif non trouvé." });
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "Utilisateur non connecté." });
                }

                if (motif.Status == MotifStatus.NonLu)
                {
                    motif.Status = MotifStatus.Lu;
                    motif.DateLecture = DateTime.Now;
                    motif.ReadBy = user.NomPrenom ?? user.UserName ?? "Unknown";

                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Motif {Id} marked as read by {User}", id, user.UserName);
                }

                return Json(new { success = true, message = "Motif marqué comme lu." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking motif as read");
                return Json(new { success = false, message = "Erreur lors de la mise à jour." });
            }
        }

        // GET: Motif/Details
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var motif = await _context.Motifs
                    .Where(m => m.Id == id)
                    .Select(m => new MotifViewModel
                    {
                        Id = m.Id,
                        NumeroEtatVenteArrivee = m.NumeroEtatVenteArrivee,
                        Description = m.Description,
                        DateCreation = m.DateCreation,
                        CreatedBy = m.CreatedBy,
                        Status = m.Status,
                        DateLecture = m.DateLecture,
                        ReadBy = m.ReadBy
                    })
                    .FirstOrDefaultAsync();

                if (motif == null)
                {
                    return NotFound();
                }

                return PartialView("_MotifDetails", motif);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading motif details");
                return StatusCode(500, "Erreur lors du chargement des détails.");
            }
        }

        // GET: API endpoint for DataTables
        [HttpGet]
        public async Task<IActionResult> GetMotifs()
        {
            try
            {
                var motifs = await _context.Motifs
                    .OrderByDescending(m => m.DateCreation)
                    .Select(m => new
                    {
                        id = m.Id,
                        numeroEtatVenteArrivee = m.NumeroEtatVenteArrivee,
                        description = m.Description.Length > 50 ?
                            m.Description.Substring(0, 50) + "..." : m.Description,
                        dateCreation = m.DateCreation.ToString("dd/MM/yyyy HH:mm"),
                        createdBy = m.CreatedBy,
                        status = (int)m.Status,
                        statusText = m.Status == MotifStatus.NonLu ? "Non Lu" :
                                   m.Status == MotifStatus.Lu ? "Lu" : "Traité",
                        dateLecture = m.DateLecture.HasValue ?
                            m.DateLecture.Value.ToString("dd/MM/yyyy HH:mm") : "",
                        readBy = m.ReadBy ?? ""
                    })
                    .ToListAsync();

                return Json(motifs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching motifs for DataTable");
                return StatusCode(500, new { error = "Erreur serveur" });
            }
        }

        // DELETE: Motif/Delete (For ResponsableCatering only)
        [HttpPost]
        [Authorize(Roles = "ResponsableCatering")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var motif = await _context.Motifs.FindAsync(id);
                if (motif == null)
                {
                    return Json(new { success = false, message = "Motif non trouvé." });
                }

                _context.Motifs.Remove(motif);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Motif {Id} deleted successfully", id);
                return Json(new { success = true, message = "Motif supprimé avec succès." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting motif {Id}", id);
                return Json(new { success = false, message = "Erreur lors de la suppression." });
            }
        }
    }
}
