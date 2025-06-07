using Microsoft.EntityFrameworkCore;
using VAB.Data;
using VAB.Models;

namespace VAB.Services
{
    public interface ITrouDeCaisseService
    {
        Task<List<TrouDeCaisse>> DetectAllShortagesAsync();
        Task<TrouDeCaisse> DetectShortageForEtatAsync(int etatId);
        Task<bool> ApplyToCommissionAsync(int trouId);
        Task<List<TrouDeCaisse>> GetShortagesForPeriodAsync(DateTime? dateDebut, DateTime? dateFin);
        Task<decimal> CalculateShortageForEtatAsync(int etatId);
    }

    public class TrouDeCaisseService : ITrouDeCaisseService
    {
        private readonly ApplicationDbContext _context;

        public TrouDeCaisseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<TrouDeCaisse>> DetectAllShortagesAsync()
        {
            var etatsToCheck = await _context.EtatDesVentesArrivee
                .Include(e => e.ArticleVentes)
                .Include(e => e.Equipage)
                    .ThenInclude(eq => eq.Pnc1)
                .Where(e => e.IsValidated &&
                           e.TotalEncaisse.HasValue &&
                           !_context.TrouDeCaisses.Any(t => t.EtatDesVentesArriveeId == e.Id))
                .ToListAsync();

            var detectedShortages = new List<TrouDeCaisse>();

            foreach (var etat in etatsToCheck)
            {
                var shortage = await CreateShortageIfExists(etat);
                if (shortage != null)
                {
                    detectedShortages.Add(shortage);
                }
            }

            if (detectedShortages.Any())
            {
                await _context.SaveChangesAsync();
            }

            return detectedShortages;
        }

        public async Task<TrouDeCaisse> DetectShortageForEtatAsync(int etatId)
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
                .Include(t => t.PncResponsable)
                .FirstOrDefaultAsync(t => t.EtatDesVentesArriveeId == etatId);

            if (existingTrou != null)
                return existingTrou;

            var shortage = await CreateShortageIfExists(etat);
            if (shortage != null)
            {
                await _context.SaveChangesAsync();
                await _context.Entry(shortage)
                    .Reference(t => t.PncResponsable)
                    .LoadAsync();
            }

            return shortage;
        }

        public async Task<bool> ApplyToCommissionAsync(int trouId)
        {
            var trou = await _context.TrouDeCaisses.FindAsync(trouId);
            if (trou == null || trou.EstApplique)
                return false;

            trou.EstApplique = true;
            trou.DateApplication = DateTime.Now;
            trou.Statut = "Appliqué";

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<TrouDeCaisse>> GetShortagesForPeriodAsync(DateTime? dateDebut, DateTime? dateFin)
        {
            var query = _context.TrouDeCaisses
                .Include(t => t.EtatDesVentesArrivee)
                .Include(t => t.PncResponsable)
                .AsQueryable();

            if (dateDebut.HasValue)
                query = query.Where(t => t.DateDetection >= dateDebut.Value);

            if (dateFin.HasValue)
                query = query.Where(t => t.DateDetection <= dateFin.Value);

            return await query.OrderByDescending(t => t.DateDetection).ToListAsync();
        }

        public async Task<decimal> CalculateShortageForEtatAsync(int etatId)
        {
            var etat = await _context.EtatDesVentesArrivee
                .Include(e => e.ArticleVentes)
                .FirstOrDefaultAsync(e => e.Id == etatId);

            if (etat == null || !etat.TotalEncaisse.HasValue)
                return 0;

            var calculatedTotal = etat.ArticleVentes.Sum(av => av.QuantiteVendu * av.PrixUnitaireHT);
            var actualTotal = etat.TotalEncaisse.Value;
            var shortage = calculatedTotal - actualTotal;

            return shortage > 0 ? shortage : 0;
        }

        private async Task<TrouDeCaisse> CreateShortageIfExists(EtatDesVentesArrivee etat)
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
                    DetectePar = "System",
                    Statut = "En Attente",
                    EstApplique = false
                };

                _context.TrouDeCaisses.Add(trouDeCaisse);

                // Update EcartMoins in EtatDesVentesArrivee if not already set
                if (!etat.EcartMoins.HasValue || etat.EcartMoins.Value != shortage)
                {
                    etat.EcartMoins = shortage;
                }

                return trouDeCaisse;
            }

            return null;
        }
    }
}
