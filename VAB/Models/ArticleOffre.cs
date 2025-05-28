using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace VAB.Models
{
    public class ArticleOffre
    {
        public int Id { get; set; }

        [Required]
        public int EtatDesOffresArriveeId { get; set; }

        [Required]
        public int ArticleId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "La quantité dotation doit être positive ou zéro.")]
        public int QuantiteDotation { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "La quantité complémentaire doit être positive ou zéro.")]
        public int QuantiteComplementaire { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "La quantité cassée doit être positive ou zéro.")]
        public int QuantiteCasse { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "La quantité offerte doit être positive ou zéro.")]
        public int QuantiteOffert { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "La quantité retour consigne doit être positive ou zéro.")]
        public int RetourConsig { get; set; }

        public Article Article { get; set; }

        public EtatDesOffresArrivee EtatDesOffresArrivee { get; set; }
    }
}
