using System.ComponentModel.DataAnnotations;

namespace VAB.ViewModel
{
    public class PrixViewModel
    {
        public int Id { get; set; }
        public int ArticleId { get; set; }
        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        [DataType(DataType.Currency)]
        public decimal Valeur { get; set; }
        [Required(ErrorMessage = "Start date is required")]
        [DataType(DataType.Date)]
        public DateTime DateDebut { get; set; }
        [DataType(DataType.Date)]
        public DateTime? DateFin { get; set; }
    }
}
