using System;
using System.ComponentModel.DataAnnotations;
using VAB.Models;

namespace VAB.ViewModel
{
    public class MotifViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le numéro d'état de vente est requis")]
        [Display(Name = "Numéro État de Vente Arrivée")]
        public string NumeroEtatVenteArrivee { get; set; }

        [Required(ErrorMessage = "La description du motif est requise")]
        [StringLength(500, ErrorMessage = "La description ne peut pas dépasser 500 caractères")]
        [Display(Name = "Description du Motif")]
        public string Description { get; set; }

        public DateTime DateCreation { get; set; }
        public string CreatedBy { get; set; }
        public MotifStatus Status { get; set; }
        public DateTime? DateLecture { get; set; }
        public string ReadBy { get; set; }

        // For display purposes
        public string StatusText => Status switch
        {
            MotifStatus.NonLu => "Non Lu",
            MotifStatus.Lu => "Lu",
            MotifStatus.Traite => "Traité",
            _ => "Inconnu"
        };

        public string StatusClass => Status switch
        {
            MotifStatus.NonLu => "badge-warning",
            MotifStatus.Lu => "badge-info",
            MotifStatus.Traite => "badge-success",
            _ => "badge-secondary"
        };
    }

    public class CreateMotifViewModel
    {
        [Required(ErrorMessage = "Le numéro d'état de vente est requis")]
        [Display(Name = "Numéro État de Vente Arrivée")]
        public string NumeroEtatVenteArrivee { get; set; }

        [Required(ErrorMessage = "La description du motif est requise")]
        [StringLength(500, ErrorMessage = "La description ne peut pas dépasser 500 caractères")]
        [Display(Name = "Description du Motif")]
        public string Description { get; set; }
    }
}
