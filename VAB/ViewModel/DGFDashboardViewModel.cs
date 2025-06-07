using System.ComponentModel.DataAnnotations;

namespace VAB.ViewModel
{
    public class DGFDashboardViewModel
    {
        [Display(Name = "États des Ventes Validés")]
        public List<EtatDesVentesValidatedViewModel> EtatsValidated { get; set; } = new List<EtatDesVentesValidatedViewModel>();

        [Display(Name = "Commissions Calculées")]
        public List<PncCommissionSummaryViewModel> CommissionsCalculees { get; set; } = new List<PncCommissionSummaryViewModel>();

        [Display(Name = "Total des Commissions")]
        [DataType(DataType.Currency)]
        public decimal TotalCommissions { get; set; }

        [Display(Name = "Période")]
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
        public decimal ExchangeRate { get; set; }

    }

    public class EtatDesVentesValidatedViewModel
    {
        public int Id { get; set; }
        public string Numero { get; set; }
        public DateTime DateEdition { get; set; }
        public string EquipageInfo { get; set; }
        public decimal TotalEncaisse { get; set; }
        public bool CommissionCalculee { get; set; }
    }

    public class PncCommissionSummaryViewModel
    {
        public int EtatDesVentesArriveeId { get; set; }
        public string NumeroEtat { get; set; }
        public DateTime DateCalcul { get; set; }
        public decimal TotalEncaisse { get; set; }
        public decimal TotalCommissions { get; set; }
        public int NombrePncs { get; set; }
    }
}
