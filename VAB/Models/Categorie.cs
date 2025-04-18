using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace VAB.Models
{
    public class Categorie
    {
        public int Id { get; set; }

        [Required]
        public string Description { get; set; }

        public ICollection<Article> Articles { get; set; }

        // New added attributes
        public int QuantiteDotation { get; set; }
        public int QuantiteComplementaire { get; set; }
        public int TotalEmbarque { get; set; }
        public int QuantiteCasse { get; set; }
        public int QuantiteOffert { get; set; }
        public int QuantiteVendu { get; set; }

        [DataType(DataType.Currency)]
        [Precision(18, 2)]
        public decimal PrixUnitaireHT { get; set; }

        [DataType(DataType.Currency)]
        [Precision(18, 2)]
        public decimal Valeur { get; set; }

        public int Restant { get; set; }
    }
}
