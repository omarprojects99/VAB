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

namespace VAB.Controllers
{
    [Authorize(Roles = "AgentSaisie")]
    public class AgentSaisieController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public AgentSaisieController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: AgentSaisie/AgentDashboard
        public IActionResult AgentDashboard()
        {
            var reports = new List<object>
            {
                new { Id = 1, Date = "2023-10-01", PNCName = "Salah Ali", TotalItemsSold = 10, TotalRevenue = 100.00m },
                new { Id = 2, Date = "2023-10-02", PNCName = "Mohamed Ahmed", TotalItemsSold = 15, TotalRevenue = 150.00m }
            };
            return View(reports);
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
            var reports = new List<object>
            {
                new { Id = 1,numero = 2542525, Date = "2023-10-01", PNCName = "Salah Ali", TotalItemsSold = 10, TotalRevenue = 100.00m },
                new { Id = 2,numero = 236757, Date = "2023-10-02", PNCName = "Mohamed Ahmed", TotalItemsSold = 15, TotalRevenue = 150.00m }
            };
            return View(reports);
        }
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

            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = "Veuillez corriger les erreurs dans le formulaire.";
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
                var f = await _context.FeuilleDeLignes.FirstOrDefaultAsync(x => x.Numero == numero);
                if (f == null)
                {
                    ViewBag.ErrorMessage = $"Feuille de Lignes avec Numéro {numero} n'existe pas.";
                    return PartialView("AddEtatDeVenteArrivee", vm);
                }
                feuilles.Add(f);
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
                foreach (var dynamicPnc in vm.DynamicPncs)
                {
                    if (string.IsNullOrEmpty(dynamicPnc.Name) || string.IsNullOrEmpty(dynamicPnc.Matricule))
                    {
                        ViewBag.ErrorMessage = "Veuillez fournir à la fois le nom et le matricule pour chaque PNC dynamique.";
                        return PartialView("AddEtatDeVenteArrivee", vm);
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
            }

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

            // Add Dynamic PNCs to PncEquipages
            foreach (var pncId in dynamicPncIds)
            {
                var pncEquipage = new PncEquipage
                {
                    PncId = pncId,
                    EquipageId = equipage.EquipageId
                };
                _context.PncEquipages.Add(pncEquipage);
            }

            // Create EtatDesVentesArrivee
            var etat = new EtatDesVentesArrivee
            {
                Numero = vm.Numero,
                DateEdition = vm.DateEdition,
                EquipageId = equipage.EquipageId,
                AgentSaisieId = user.Id,
                FeuilleEtatDesVentes = new List<FeuilleEtatDesVentesArrivee>()
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

            try
            {
                _context.EtatDesVentesArrivee.Add(etat);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Erreur lors de l'enregistrement : {ex.Message}";
                return PartialView("AddEtatDeVenteArrivee", vm);
            }

            ViewBag.Success = true;
            return PartialView("AddEtatDeVenteArrivee", vm);
        }

        // GET: DataTable AJAX source
        [HttpGet]
        public IActionResult GetEtatDesVentesArrivee()
        {
            var etats = _context.EtatDesVentesArrivee
                .Include(e => e.AgentSaisie)

                .Select(e => new
                {
                    e.Id,
                    numero = e.Numero,
                    dateEdition = e.DateEdition,
                    agentSaisie = e.AgentSaisie != null ? e.AgentSaisie.NomPrenom : "Unknown",


                })
                .ToList();
            return Json(etats);
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
                .FirstOrDefaultAsync(e => e.Id == id && e.AgentSaisieId == user.Id);

            if (etat == null)
            {
                return NotFound("État des Ventes non trouvé ou vous n'êtes pas autorisé à le voir.");
            }

            var vm = new EtatDesVentesArriveeViewModel
            {
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
                DynamicPncs = etat.Equipage.PncEquipages.Select(pe => new DynamicPnc
                {
                    Name = $"{pe.Pnc.Nom} {pe.Pnc.Prenom}",
                    Matricule = pe.Pnc.Matricule
                }).ToList()
            };

            return PartialView("ViewEtatDeVenteArrivee", vm);
        }
    }
}