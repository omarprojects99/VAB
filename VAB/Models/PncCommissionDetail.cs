using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace VAB.Models
{
    public class PncCommissionDetail
    {
        public int Id { get; set; }

        [Required]
        public int PncCommissionCalculationId { get; set; }
        public PncCommissionCalculation PncCommissionCalculation { get; set; }

        [Required]
        public int PncId { get; set; }
        public Pnc Pnc { get; set; }

        public bool IsPnc1 { get; set; }

        [DataType(DataType.Currency)]
        [Precision(18, 2)]
        public decimal CommissionIndividuelle { get; set; } // 1% if PNC1, 0 otherwise

        [DataType(DataType.Currency)]
        [Precision(18, 2)]
        public decimal CommissionCollective { get; set; } // Share from 15%

        [DataType(DataType.Currency)]
        [Precision(18, 2)]
        public decimal CommissionTotale { get; set; } // Total commission for this PNC
    }
}
