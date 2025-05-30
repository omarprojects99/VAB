using System.ComponentModel.DataAnnotations;

namespace VAB.Models
{
    public class ComparisonArticle
    {
        [Key]
        public int Id { get; set; }

        public int SalesComparisonId { get; set; }
        public SalesComparison SalesComparison { get; set; }

        public int ArticleId { get; set; }
        public int IdArticle { get; set; }
        public string Description { get; set; }

        public int QuantiteVenduArrivee { get; set; }
        public int QuantiteVenduDepart { get; set; }
        public bool QuantiteVenduMatch { get; set; }

        public decimal PrixUnitaireArrivee { get; set; }
        public decimal PrixUnitaireDepart { get; set; }
        public bool PrixUnitaireMatch { get; set; }

        public decimal ValeurArrivee { get; set; }
        public decimal ValeurDepart { get; set; }
        public bool ValeurMatch { get; set; }
    }
}