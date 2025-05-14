using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace VAB.Models
{
    public class ArticleVente
    {
        public int Id { get; set; }
        public int EtatDesVentesArriveeId { get; set; }
        public int ArticleId { get; set; }
        public int QuantiteDotation { get; set; }
        public int QuantiteComplementaire { get; set; }
        public int QuantiteCasse { get; set; }
        public int QuantiteOffert { get; set; }
        public int QuantiteVendu { get; set; }
        [DataType(DataType.Currency)]
        [Precision(18, 2)]
        public decimal PrixUnitaireHT { get; set; }

        public EtatDesVentesArrivee EtatDesVentesArrivee { get; set; }
        public Article Article { get; set; }

    }
}
