using System.ComponentModel.DataAnnotations;

namespace VAB.ViewModel
{
    public class PrixViewModel
    {
        public int Id { get; set; }
        public int ArticleId { get; set; }
        [Required(ErrorMessage = "Price est requis")]
        [Range(0.01, double.MaxValue, ErrorMessage = " Le prix doit être un nombre positif valide")]
        [DataType(DataType.Currency)]
        public decimal Valeur { get; set; }
        [Required(ErrorMessage = "date de lancement est requis")]
        [DataType(DataType.Date)]
        public DateTime DateDebut { get; set; }
        [DataType(DataType.Date)]
        public DateTime? DateFin { get; set; }
    }
}
