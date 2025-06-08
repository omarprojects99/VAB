using VAB.Models;

namespace VAB.Services
{
    public interface IRedevanceAnalyticsService
    {
        Task<RedevanceAnalytics> GetAdvancedAnalyticsAsync(int annee);
        Task<List<TopSellingProduct>> GetTopSellingProductsAsync(int? mois = null, int? annee = null, int top = 10);
        Task<List<CrewPerformance>> GetCrewPerformanceAsync(int annee);
        Task<PredictionData> GetPredictionsAsync();
        Task<List<MonthlyRedevance>> GetMonthlyTrendsAsync(int annee);
        Task<Dictionary<string, object>> GetRealTimeStatsAsync();
        Task<List<object>> GetChartDataAsync(string chartType, int annee);
        Task<List<EtatDesVentesArrivee>> GetValidatedStatesForPeriodAsync(int mois, int annee);
    }
}
