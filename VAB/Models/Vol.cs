using System.ComponentModel.DataAnnotations;

namespace VAB.Models
{
    public class Vol
    {
        public int Id { get; set; }

        [Required]
        public int Numero { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required]
        public string Destination { get; set; }

        public string Aircraft { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan Heure { get; set; }

        [Required]
        public string TypeVol { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int NombrePassagers { get; set; }

        public int? FeuilleDeLignesId { get; set; }
        public FeuilleDeLignes FeuilleDeLignes { get; set; }

        public int? EtatDesVentesArriveeId { get; set; }
        public EtatDesVentesArrivee EtatDesVentesArrivee { get; set; }
    }
}