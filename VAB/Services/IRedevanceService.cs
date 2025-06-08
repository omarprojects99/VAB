using VAB.Models;

namespace VAB.Services
{
    public interface IRedevanceService
    {
        Task<bool> CalculateRedevanceAsync(int mois, int annee, string userId);
        Task<List<Redevance>> GetAllRedevancesAsync();
        Task<Redevance> GetRedevanceAsync(int id);
        Task<bool> ValidateRedevanceAsync(int id);
        Task<bool> DeleteRedevanceAsync(int id);
        Task<decimal> GetTotalRedevanceForYearAsync(int annee);
    }
}
