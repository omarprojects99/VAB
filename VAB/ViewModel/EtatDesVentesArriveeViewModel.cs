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

    }
    public class DynamicPnc
    {
        public string Name { get; set; }
        public string Matricule { get; set; }
    }
}
