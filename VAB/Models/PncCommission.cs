using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace VAB.Models
{
    public class PncCommission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EtatDesVentesArriveeId { get; set; }
        public EtatDesVentesArrivee EtatDesVentesArrivee { get; set; }

        [Required]
        public int PncId { get; set; }
        public Pnc Pnc { get; set; }

        [Required]
        [Precision(18, 2)]
        public decimal Commission { get; set; }

        [Required]
        public DateTime DateCalculated { get; set; }
    }
}