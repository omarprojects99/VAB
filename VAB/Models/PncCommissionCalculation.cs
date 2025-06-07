using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace VAB.Models
{
    public class PncCommissionCalculation
    {
        public int Id { get; set; }

        [Required]
        public int EtatDesVentesArriveeId { get; set; }
        public EtatDesVentesArrivee EtatDesVentesArrivee { get; set; }

        [Required]
        public DateTime DateCalcul { get; set; }

        [DataType(DataType.Currency)]
        [Precision(18, 2)]
        public decimal TotalEncaisse { get; set; }

        [DataType(DataType.Currency)]
        [Precision(18, 2)]
        public decimal CommissionPnc1Individual { get; set; } // 1% for PNC1

        [DataType(DataType.Currency)]
        [Precision(18, 2)]
        public decimal CommissionCollective { get; set; } // 15% total for all PNCs

        [DataType(DataType.Currency)]
        [Precision(18, 2)]
        public decimal CommissionPnc1FromCollective { get; set; } // PNC1's share from 15%

        [DataType(DataType.Currency)]
        [Precision(18, 2)]
        public decimal CommissionTotalPnc1 { get; set; } // Total for PNC1 (1% + share of 15%)

        public int NombrePncsTotal { get; set; }

        [DataType(DataType.Currency)]
        [Precision(18, 2)]
        public decimal CommissionParPnc { get; set; } // Individual share from 15% for each PNC

        public string CalculatedBy { get; set; }

        public List<PncCommissionDetail> PncCommissionDetails { get; set; } = new List<PncCommissionDetail>();
    }
}
