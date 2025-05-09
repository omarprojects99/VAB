using System.ComponentModel.DataAnnotations;

namespace VAB.Models
{
    public class Vol
    {
         public int Id { get; set; }

        [Required]
        public int Numero { get; set; } // Flight number

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } // Flight date

        [Required]
        public string Destination { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan Heure { get; set; } // Flight time

        [Required]
        public string TypeVol { get; set; } // e.g., "Commercial", "Mise en place"

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Please enter a valid number of passengers")]
        public int NombrePassagers { get; set; } // ✅ Number of passengers

        // Foreign key for FeuilleDeLignes
        public int? FeuilleDeLignesId { get; set; }
        public FeuilleDeLignes FeuilleDeLignes { get; set; }
    }
}