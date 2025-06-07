using System.ComponentModel.DataAnnotations;

namespace VAB.ViewModel
{
    public class PncBilanViewModel
    {
        [Display(Name = "PNC")]
        public int PncId { get; set; }
        public string PncNom { get; set; }
        public string PncPrenom { get; set; }
        public string PncMatricule { get; set; }
        public string PncNomComplet => $"{PncNom} {PncPrenom}";

        [Display(Name = "Période")]
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }

        [Display(Name = "Transactions")]
        public List<PncTransactionViewModel> Transactions { get; set; } = new List<PncTransactionViewModel>();

        [Display(Name = "Solde Initial")]
        [DataType(DataType.Currency)]
        public decimal SoldeInitial { get; set; }

        [Display(Name = "Total Crédits")]
        [DataType(DataType.Currency)]
        public decimal TotalCredits { get; set; }

        [Display(Name = "Total Débits")]
        [DataType(DataType.Currency)]
        public decimal TotalDebits { get; set; }

        [Display(Name = "Solde Final")]
        [DataType(DataType.Currency)]
        public decimal SoldeFinal { get; set; }

        [Display(Name = "Nombre de Transactions")]
        public int NombreTransactions { get; set; }
    }

    public class PncTransactionViewModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string TypeTransaction { get; set; } // "Commission", "Trou de Caisse", "Ajustement"
        public string Description { get; set; }
        public string Reference { get; set; } // Numéro d'état, etc.

        [DataType(DataType.Currency)]
        public decimal? Debit { get; set; }

        [DataType(DataType.Currency)]
        public decimal? Credit { get; set; }

        [DataType(DataType.Currency)]
        public decimal SoldeApres { get; set; }

        public string Statut { get; set; }
        public string CreePar { get; set; }
        public string Commentaires { get; set; }

        // Additional info
        public int? EtatDesVentesArriveeId { get; set; }
        public int? TrouDeCaisseId { get; set; }
        public int? CommissionCalculationId { get; set; }
    }

    public class PncBilanDashboardViewModel
    {
        [Display(Name = "Période")]
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }

        [Display(Name = "PNCs avec Transactions")]
        public List<PncBilanSummaryViewModel> PncSummaries { get; set; } = new List<PncBilanSummaryViewModel>();

        [Display(Name = "Total Général Crédits")]
        [DataType(DataType.Currency)]
        public decimal TotalGeneralCredits { get; set; }

        [Display(Name = "Total Général Débits")]
        [DataType(DataType.Currency)]
        public decimal TotalGeneralDebits { get; set; }

        [Display(Name = "Solde Général")]
        [DataType(DataType.Currency)]
        public decimal SoldeGeneral { get; set; }
    }

    public class PncBilanSummaryViewModel
    {
        public int PncId { get; set; }
        public string PncNomComplet { get; set; }
        public string PncMatricule { get; set; }

        [DataType(DataType.Currency)]
        public decimal TotalCredits { get; set; }

        [DataType(DataType.Currency)]
        public decimal TotalDebits { get; set; }

        [DataType(DataType.Currency)]
        public decimal SoldeFinal { get; set; }

        public int NombreTransactions { get; set; }
        public DateTime? DerniereTransaction { get; set; }
    }
}
