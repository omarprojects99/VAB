using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VAB.Models
{
    public class OffersComparison
    {
        public int Id { get; set; }
        [Required]
        public string Numero { get; set; }
        public DateTime ComparisonDate { get; set; }
        public List<ComparisonOfferArticle> Articles { get; set; } = new List<ComparisonOfferArticle>();
    }

    public class ComparisonOfferArticle
    {
        public int Id { get; set; }
        public int OffersComparisonId { get; set; }
        public int ArticleId { get; set; }
        public int IdArticle { get; set; }
        public string Description { get; set; }
        public int QuantiteDotationArrivee { get; set; }
        public int QuantiteDotationDepart { get; set; }
        public bool QuantiteDotationMatch { get; set; }
        public int QuantiteComplementaireArrivee { get; set; }
        public int QuantiteComplementaireDepart { get; set; }
        public bool QuantiteComplementaireMatch { get; set; }
        public int QuantiteCasseArrivee { get; set; }
        public int QuantiteCasseDepart { get; set; }
        public bool QuantiteCasseMatch { get; set; }
        public int QuantiteOffertArrivee { get; set; }
        public int QuantiteOffertDepart { get; set; }
        public bool QuantiteOffertMatch { get; set; }
        public int RetourConsigArrivee { get; set; }
        public int RetourConsigDepart { get; set; }
        public bool RetourConsigMatch { get; set; }
    }

    public class ComparisonOfferArticleDto
    {
        public int ArticleId { get; set; }
        public int IdArticle { get; set; }
        public string Description { get; set; }
        public bool QuantiteDotationMatch { get; set; }
        public int QuantiteDotationArrivee { get; set; }
        public int QuantiteDotationDepart { get; set; }
        public bool QuantiteComplementaireMatch { get; set; }
        public int QuantiteComplementaireArrivee { get; set; }
        public int QuantiteComplementaireDepart { get; set; }
        public bool QuantiteCasseMatch { get; set; }
        public int QuantiteCasseArrivee { get; set; }
        public int QuantiteCasseDepart { get; set; }
        public bool QuantiteOffertMatch { get; set; }
        public int QuantiteOffertArrivee { get; set; }
        public int QuantiteOffertDepart { get; set; }
        public bool RetourConsigMatch { get; set; }
        public int RetourConsigArrivee { get; set; }
        public int RetourConsigDepart { get; set; }
    }
}