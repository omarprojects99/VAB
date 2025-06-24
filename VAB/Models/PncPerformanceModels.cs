using System.ComponentModel.DataAnnotations;

namespace VAB.Models
{
    public class PncPerformanceAnalytics
    {
        public decimal TotalCommissionsAnnuelles { get; set; }
        public decimal MoyenneMensuelle { get; set; }
        public decimal CroissanceAnnuelle { get; set; }
        public string MeilleurMois { get; set; }
        public string PireMois { get; set; }
        public List<MonthlyPncPerformance> PerformancesMensuelles { get; set; } = new();
        public List<TopPncPerformance> TopPncs { get; set; } = new();
        public List<PncCommissionTrend> TendanceCommissions { get; set; } = new();
        public PncPredictionData Predictions { get; set; }
    }

    public class MonthlyPncPerformance
    {
        public int Mois { get; set; }
        public int Annee { get; set; }
        public string MoisNom { get; set; }
        public decimal TotalCommissions { get; set; }
        public decimal TotalTrousDeCaisse { get; set; }
        public int NombrePncsActifs { get; set; }
        public decimal CroissanceVsPrecedent { get; set; }
        public decimal PerformanceNette { get; set; }
    }

    public class TopPncPerformance
    {
        public int PncId { get; set; }
        public string PncInfo { get; set; } // "Nom Prenom, Matricule"
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string Matricule { get; set; }
        public decimal TotalCommissions { get; set; }
        public decimal TotalTrousDeCaisse { get; set; }
        public decimal PerformanceNette { get; set; }
        public int NombreCalculs { get; set; }
        public decimal CommissionMoyenneParCalcul { get; set; }
        public string MeilleurMois { get; set; }
        public decimal TauxPerformance { get; set; } // Percentage score
        public string Telephone { get; set; }
    }

    public class PncCommissionTrend
    {
        public DateTime Date { get; set; }
        public decimal Valeur { get; set; }
        public string Type { get; set; } // "Commission" or "TrouDeCaisse"
        public int PncId { get; set; }
        public string PncNom { get; set; }
    }

    public class PncPredictionData
    {
        public decimal CommissionPrevueMoisProchain { get; set; }
        public decimal CommissionPrevueAnnee { get; set; }
        public string Tendance { get; set; }
        public decimal Confiance { get; set; }
        public List<string> Recommandations { get; set; } = new();
    }
}
