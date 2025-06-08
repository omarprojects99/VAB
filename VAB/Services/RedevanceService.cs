using Microsoft.EntityFrameworkCore;
using VAB.Data;
using VAB.Models;

namespace VAB.Services
{
    public class RedevanceService : IRedevanceService
    {
        private readonly ApplicationDbContext _context;
        private const decimal REDEVANCE_PERCENTAGE = 0.40m;

        public RedevanceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CalculateRedevanceAsync(int mois, int annee, string userId)
        {
            try
            {
                // Get all validated EtatDesVentesArrivee for the period
                var etatsVentes = await _context.EtatDesVentesArrivee
                    .Include(e => e.ArticleVentes)
                    .Where(e => e.IsValidated &&
                               e.DateEdition.Month == mois &&
                               e.DateEdition.Year == annee)
                    .ToListAsync();

                if (!etatsVentes.Any())
                    return false;

                // Delete existing redevance for this period if exists
                var existingRedevance = await _context.Redevances
                    .Include(r => r.RedevanceDetails)
                    .FirstOrDefaultAsync(r => r.Mois == mois && r.Annee == annee);

                if (existingRedevance != null)
                {
                    _context.Redevances.Remove(existingRedevance);
                }

                // Calculate totals
                decimal totalVentes = 0;
                var redevanceDetails = new List<RedevanceDetail>();

                foreach (var etat in etatsVentes)
                {
                    // Calculate total sales for this etat
                    decimal etatTotal = etat.ArticleVentes
                        .Sum(av => av.QuantiteVendu * av.PrixUnitaireHT);

                    totalVentes += etatTotal;

                    var detail = new RedevanceDetail
                    {
                        EtatDesVentesArriveeId = etat.Id,
                        TotalVentes = etatTotal,
                        MontantRedevance = etatTotal * REDEVANCE_PERCENTAGE,
                        NumeroEtat = etat.Numero,
                        DateEtat = etat.DateEdition,
                        NombreArticles = etat.ArticleVentes.Count
                    };

                    redevanceDetails.Add(detail);
                }

                // Create new redevance
                var redevance = new Redevance
                {
                    Mois = mois,
                    Annee = annee,
                    TotalVentes = totalVentes,
                    MontantRedevance = totalVentes * REDEVANCE_PERCENTAGE,
                    DateCalcul = DateTime.Now,
                    NombreEtats = etatsVentes.Count,
                    CalculeParId = userId,
                    IsValidated = false,
                    RedevanceDetails = redevanceDetails
                };

                _context.Redevances.Add(redevance);
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Redevance>> GetAllRedevancesAsync()
        {
            return await _context.Redevances
                .Include(r => r.CalculePar)
                .OrderByDescending(r => r.Annee)
                .ThenByDescending(r => r.Mois)
                .ToListAsync();
        }

        public async Task<Redevance> GetRedevanceAsync(int id)
        {
            return await _context.Redevances
                .Include(r => r.RedevanceDetails)
                    .ThenInclude(rd => rd.EtatDesVentesArrivee)
                .Include(r => r.CalculePar)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<bool> ValidateRedevanceAsync(int id)
        {
            var redevance = await _context.Redevances.FindAsync(id);
            if (redevance == null || redevance.IsValidated)
                return false;

            redevance.IsValidated = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteRedevanceAsync(int id)
        {
            var redevance = await _context.Redevances
                .Include(r => r.RedevanceDetails)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (redevance == null || redevance.IsValidated)
                return false;

            _context.Redevances.Remove(redevance);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<decimal> GetTotalRedevanceForYearAsync(int annee)
        {
            return await _context.Redevances
                .Where(r => r.Annee == annee && r.IsValidated)
                .SumAsync(r => r.MontantRedevance);
        }
    }
}
