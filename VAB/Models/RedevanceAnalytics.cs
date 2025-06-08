using System.ComponentModel.DataAnnotations;

namespace VAB.Models
{
    public class RedevanceAnalytics
    {
        public decimal TotalRedevanceAnnuelle { get; set; }
        public decimal MoyenneMensuelle { get; set; }
        public decimal CroissanceAnnuelle { get; set; }
        public string MeilleurMois { get; set; }
        public string PireMois { get; set; }
        public List<MonthlyRedevance> RedevancesMensuelles { get; set; } = new();
        public List<TopSellingProduct> TopProduits { get; set; } = new();
        public List<CrewPerformance> PerformanceEquipages { get; set; } = new();
        public List<TrendData> TendanceVentes { get; set; } = new();
        public PredictionData Predictions { get; set; }
    }

    public class MonthlyRedevance
    {
        public int Mois { get; set; }
        public int Annee { get; set; }
        public string MoisNom { get; set; }
        public decimal Montant { get; set; }
        public decimal TotalVentes { get; set; }
        public int NombreEtats { get; set; }
        public decimal CroissanceVsPrecedent { get; set; }
        public bool IsValidated { get; set; }
    }

    public class TopSellingProduct
    {
        public int ArticleId { get; set; }
        public string Description { get; set; }
        public string Categorie { get; set; }
        public int QuantiteTotale { get; set; }
        public decimal ValeurTotale { get; set; }
        public decimal RedevanceGeneree { get; set; }
        public int NombreVentes { get; set; }
        public decimal PrixMoyen { get; set; }
    }

    public class CrewPerformance
    {
        public string EquipageInfo { get; set; }
        public decimal TotalVentes { get; set; }
        public decimal RedevanceGeneree { get; set; }
        public int NombreVols { get; set; }
        public decimal VenteMoyenneParVol { get; set; }
        public string MeilleurMois { get; set; }
    }

    public class TrendData
    {
        public DateTime Date { get; set; }
        public decimal Valeur { get; set; }
        public string Type { get; set; }
    }

    public class PredictionData
    {
        public decimal RedevancePrevueMoisProchain { get; set; }
        public decimal RedevancePrevueAnnee { get; set; }
        public string Tendance { get; set; }
        public decimal Confiance { get; set; }
        public List<string> Recommandations { get; set; } = new();
    }
}
