using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace VAB.Models
{
    public class EtatDesVentesArrivee
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
         public List<FeuilleEtatDesVentesArrivee> FeuilleEtatDesVentes { get; set; }


    }
}
