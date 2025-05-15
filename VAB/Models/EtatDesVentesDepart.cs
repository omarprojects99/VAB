using System.ComponentModel.DataAnnotations;

namespace VAB.Models
{
    public class EtatDesVentesDepart
    {
        
        public int Id { get; set; }
        public string Numero { get; set; }
        [Required(ErrorMessage = "La date et heure d'édition sont requises.")]
        [Display(Name = "Date et Heure d'Edition")]
        public DateTime DateEdition { get; set; }

        [Required]
        public int EquipageId { get; set; }
        public Equipage Equipage { get; set; }

        //[Required(ErrorMessage = "L'agent est requis.")]
        public string AgentSaisieId { get; set; }

        // Navigation property (optional, for EF Core relationships)
        public User AgentSaisie { get; set; }

        // Many-to-many with FeuilleDeLignes
        public List<FeuilleEtatDesVentesDepart> FeuilleEtatDesVentes { get; set; }

        public List<ArticleVenteDepart> ArticleVentesDepart { get; set; } = new List<ArticleVenteDepart>();

        public decimal? TotalEncaisse { get; set; }
        public decimal? EcartPlus { get; set; }
        public decimal? EcartMoins { get; set; }
    }
}
