using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace VAB.Models
{
    public class Article
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Article ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Article ID must be a positive number")]
        public int IdArticle { get; set; }

        [Required]
        public string Description { get; set; }

        public int CategorieId { get; set; } // Foreign key
        public Categorie Categorie { get; set; } // Navigation property

        public ICollection<Prix> PrixHistorique { get; set; }

        // New added attributes
        

        [DataType(DataType.Currency)]
        [Precision(18, 2)]
        public decimal PrixUnitaireHT { get; set; }

        
       
    }
}
