using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using VAB.Models;

namespace VAB.ViewModel
{
    public class EtatDesVentesArriveeViewModel
    {
        public int Id { get; set; }
        public string Numero { get; set; }
        [Required(ErrorMessage = "La date et heure d'édition sont requises.")]
        [Display(Name = "Date et Heure d'Edition")]
        public DateTime DateEdition { get; set; }
        public string AgentSaisieId { get; set; }
        public string AgentSaisieName { get; set; }

        // List of FeuilleDeLignes Numero values
        public List<int> FeuilleDeLignesNumeros { get; set; } = new List<int>();

        // Crew fields
        [Required(ErrorMessage = "Le nom du CC1 est requis.")]
        public string Cc1Name { get; set; }

        [Required(ErrorMessage = "Le matricule du CC1 est requis.")]
        public string Cc1Matricule { get; set; }

        public string Cc2Name { get; set; }
        public string Cc2Matricule { get; set; }

        [Required(ErrorMessage = "Le nom du PNC1 est requis.")]
        public string Pnc1Name { get; set; }

        [Required(ErrorMessage = "Le matricule du PNC1 est requis.")]
        public string Pnc1Matricule { get; set; }

        public string Pnc2Name { get; set; }
        public string Pnc2Matricule { get; set; }
        // Dynamic PNCs
        public List<DynamicPnc> DynamicPncs { get; set; } = new List<DynamicPnc>();
        // Vol fields (Données Générales)
        [Required(ErrorMessage = "Le numéro de vol est requis.")]
        [Display(Name = "VOL")]
        public int FlightNumber { get; set; }

        [Required(ErrorMessage = "La date du vol est requise.")]
        [Display(Name = "DATE")]
        [DataType(DataType.Date)]
        public DateTime FlightDate { get; set; }

        [Required(ErrorMessage = "La destination est requise.")]
        [Display(Name = "DEST")]
        public string Destination { get; set; }

        [Display(Name = "A/C")]
        public string Aircraft { get; set; }

        [Required(ErrorMessage = "L'heure du vol est requise.")]
        [Display(Name = "HEURE")]
        [DataType(DataType.Time)]
        public TimeSpan FlightTime { get; set; }

        [Required(ErrorMessage = "Le type de vol est requis.")]
        [Display(Name = "TYPE")]
        public string FlightType { get; set; }

        [Required(ErrorMessage = "Le nombre de passagers est requis.")]
        [Range(0, int.MaxValue, ErrorMessage = "Le nombre de passagers doit être positif.")]
        [Display(Name = "Tot Pax")]
        public int TotalPassengers { get; set; }

        public List<ArticleVenteViewModel> ArticleVentes { get; set; } = new List<ArticleVenteViewModel>();
        public List<ArticleVenteViewModel> ArticleVenteDepart { get; set; } = new List<ArticleVenteViewModel>();

        // Summary fields
        [DataType(DataType.Currency)]
        public decimal? TotalEncaisse { get; set; }

        [DataType(DataType.Currency)]
        public decimal? EcartPlus { get; set; }

        [DataType(DataType.Currency)]
        public decimal? EcartMoins { get; set; }
        public bool IsValidated { get; set; } // New field

    }
    public class ArticleVenteViewModel
    {
        public int ArticleId { get; set; }
        public int IdArticle { get; set; } // User inputs this
        public string Description { get; set; }
        public int CategorieId { get; set; }

        [Display(Name = "Catégorie")]
        public string CategorieDescription { get; set; }
        public int QuantiteDotation { get; set; }
        public int QuantiteComplementaire { get; set; }
        public int QuantiteCasse { get; set; }
        public int QuantiteOffert { get; set; }
        public int QuantiteVendu { get; set; }
        [Display(Name = "Total embarqué")]
        public int TotalEmbarque => QuantiteDotation + QuantiteComplementaire;

        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Le prix doit être un nombre décimal avec 2 décimales maximum.")]
        public decimal PrixUnitaireHT { get; set; }

        [Display(Name = "Valeur")]
        public decimal Valeur => QuantiteVendu * PrixUnitaireHT;

        [Display(Name = "Restant")]
        public int Restant => TotalEmbarque - QuantiteCasse - QuantiteOffert - QuantiteVendu;
    }
    public class DynamicPnc
    {
        public string Name { get; set; }
        public string Matricule { get; set; }
    }
}
