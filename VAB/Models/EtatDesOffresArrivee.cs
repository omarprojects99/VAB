using System.ComponentModel.DataAnnotations;

namespace VAB.Models
{
    public class EtatDesOffresArrivee
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le numéro est requis.")]
      
        public string Numero { get; set; }
        //[Required(ErrorMessage = "L'agent est requis.")]
        public string AgentSaisieId { get; set; }

        // Navigation property (optional, for EF Core relationships)
        public User AgentSaisie { get; set; }

        [Required(ErrorMessage = "La date d'édition est requise.")]
        [DataType(DataType.DateTime)]
        public DateTime DateEdition { get; set; }

        public bool IsValidated { get; set; }

        public List<ArticleOffre> ArticleOffres { get; set; } = new List<ArticleOffre>();
    }
}
