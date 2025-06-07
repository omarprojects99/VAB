using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace VAB.Models
{
    public class TrouDeCaisse
    {
        public int Id { get; set; }

        [Required]
        public int EtatDesVentesArriveeId { get; set; }
        public EtatDesVentesArrivee EtatDesVentesArrivee { get; set; }

        [Required]
        public int PncResponsableId { get; set; } // Usually PNC1
        public Pnc PncResponsable { get; set; }

        [Required]
        public DateTime DateDetection { get; set; }

        [DataType(DataType.Currency)]
        [Precision(18, 2)]
        public decimal MontantCalcule { get; set; } // Total calculated from articles sold

        [DataType(DataType.Currency)]
        [Precision(18, 2)]
        public decimal MontantEncaisse { get; set; } // Actual amount collected

        [DataType(DataType.Currency)]
        [Precision(18, 2)]
        public decimal MontantManquant { get; set; } // Difference (shortage)

        public string Motif { get; set; } // Reason/explanation

        public bool EstApplique { get; set; } // Whether applied to commission calculation

        public DateTime? DateApplication { get; set; } // When applied to commission

        public string DetectePar { get; set; } // Who detected the shortage

        public string Statut { get; set; } = "En Attente"; // En Attente, Appliqué, Contesté, Résolu

        public string Commentaires { get; set; }
    }
}
