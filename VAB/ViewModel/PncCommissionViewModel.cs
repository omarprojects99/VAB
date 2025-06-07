using System.ComponentModel.DataAnnotations;

namespace VAB.ViewModel
{
    public class PncCommissionViewModel
    {
        public int EtatDesVentesArriveeId { get; set; }
        public string NumeroEtat { get; set; }
        public DateTime DateEdition { get; set; }
        public string EquipageInfo { get; set; }

        [Display(Name = "Total Encaissé")]
        [DataType(DataType.Currency)]
        public decimal TotalEncaisse { get; set; }

        [Display(Name = "Commission PNC1 (1%)")]
        [DataType(DataType.Currency)]
        public decimal CommissionPnc1Individual { get; set; }

        [Display(Name = "Commission Collective (15%)")]
        [DataType(DataType.Currency)]
        public decimal CommissionCollective { get; set; }

        [Display(Name = "Nombre de PNCs")]
        public int NombrePncsTotal { get; set; }

        [Display(Name = "Commission par PNC")]
        [DataType(DataType.Currency)]
        public decimal CommissionParPnc { get; set; }

        public List<PncCommissionDetailViewModel> PncCommissions { get; set; } = new List<PncCommissionDetailViewModel>();
    }

    public class PncCommissionDetailViewModel
    {
        public int PncId { get; set; }
        public string NomComplet { get; set; }
        public string Matricule { get; set; }
        public bool IsPnc1 { get; set; }

        [Display(Name = "Commission Individuelle")]
        [DataType(DataType.Currency)]
        public decimal CommissionIndividuelle { get; set; }

        [Display(Name = "Commission Collective")]
        [DataType(DataType.Currency)]
        public decimal CommissionCollective { get; set; }

        [Display(Name = "Commission Totale")]
        [DataType(DataType.Currency)]
        public decimal CommissionTotale { get; set; }
    }
}
