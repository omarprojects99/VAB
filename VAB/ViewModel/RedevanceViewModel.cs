using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using VAB.Models;

namespace VAB.ViewModels
{
    public class RedevanceCalculationViewModel
    {
        [Required(ErrorMessage = "Le mois est requis")]
        [Range(1, 12, ErrorMessage = "Le mois doit être entre 1 et 12")]
        [Display(Name = "Mois")]
        public int Mois { get; set; }

        [Required(ErrorMessage = "L'année est requise")]
        [Range(2020, 2050, ErrorMessage = "L'année doit être valide")]
        [Display(Name = "Année")]
        public int Annee { get; set; }

        [Display(Name = "Recalculer si existe")]
        public bool ForceRecalculation { get; set; }

        public List<SelectListItem> MoisOptions { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "1", Text = "Janvier" },
            new SelectListItem { Value = "2", Text = "Février" },
            new SelectListItem { Value = "3", Text = "Mars" },
            new SelectListItem { Value = "4", Text = "Avril" },
            new SelectListItem { Value = "5", Text = "Mai" },
            new SelectListItem { Value = "6", Text = "Juin" },
            new SelectListItem { Value = "7", Text = "Juillet" },
            new SelectListItem { Value = "8", Text = "Août" },
            new SelectListItem { Value = "9", Text = "Septembre" },
            new SelectListItem { Value = "10", Text = "Octobre" },
            new SelectListItem { Value = "11", Text = "Novembre" },
            new SelectListItem { Value = "12", Text = "Décembre" }
        };
    }

    public class RedevanceCalculationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public decimal ChiffreAffairesTotal { get; set; }
        public decimal MontantRedevance { get; set; }
        public int NombreEtats { get; set; }
    }

    public class RedevanceStatistics
    {
        public decimal TotalRedevancesAnnee { get; set; }
        public decimal TotalChiffreAffairesAnnee { get; set; }
        public int NombreRedevancesValidees { get; set; }
        public int NombreRedevancesEnAttente { get; set; }
        public Redevance DerniereRedevance { get; set; }
    }

    public class RedevanceListViewModel
    {
        public List<Redevance> Redevances { get; set; } = new List<Redevance>();
        public int? AnneeFiltre { get; set; }
      
        public RedevanceStatistics Statistics { get; set; }
    }
}
