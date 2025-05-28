using System.ComponentModel.DataAnnotations;
using VAB.Models;

namespace VAB.ViewModel
{
    public class EtatDesOffresArriveeViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le numéro est requis.")]
        public string AgentSaisieId { get; set; }
        public string AgentSaisieName { get; set; }
        public string Numero { get; set; }

        [Required(ErrorMessage = "La date et heure d'édition sont requises.")]
        [Display(Name = "Date et Heure d'Edition")]
        public DateTime DateEdition { get; set; }

        public bool IsValidated { get; set; }

        public List<ArticleOffreViewModel> ArticleOffres { get; set; } = new List<ArticleOffreViewModel>();
    }

    public class ArticleOffreViewModel
    {
        public int ArticleId { get; set; }

        [Required(ErrorMessage = "Le code article est requis.")]
        [Range(1, int.MaxValue, ErrorMessage = "Le code article doit être un entier positif.")]
        public int IdArticle { get; set; }

        [Required(ErrorMessage = "La description est requise.")]
        public string Description { get; set; } = string.Empty;

        public int CategorieId { get; set; }

        [Display(Name = "Catégorie")]
        public string CategorieDescription { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "La quantité dotation doit être positive ou zéro.")]
        public int QuantiteDotation { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "La quantité complémentaire doit être positive ou zéro.")]
        public int QuantiteComplementaire { get; set; }

        [Display(Name = "Total embarqué")]
        public int TotalEmbarque => QuantiteDotation + QuantiteComplementaire;

        [Range(0, int.MaxValue, ErrorMessage = "La quantité cassée doit être positive ou zéro.")]
        public int QuantiteCasse { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "La quantité offerte doit être positive ou zéro.")]
        public int QuantiteOffert { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "La quantité retour consigne doit être positive ou zéro.")]
        public int RetourConsig { get; set; }

        [Display(Name = "Déficite")]
        public int Deficite => TotalEmbarque - (QuantiteCasse + QuantiteOffert + RetourConsig);

        [Display(Name = "Restant")]
        public int Restant => TotalEmbarque - QuantiteCasse - QuantiteOffert;
    }

}

