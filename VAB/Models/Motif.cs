using System;
using System.ComponentModel.DataAnnotations;

namespace VAB.Models
{
    public class Motif
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le numéro d'état de vente est requis")]
        [Display(Name = "Numéro État de Vente Arrivée")]
        public string NumeroEtatVenteArrivee { get; set; }

        [Required(ErrorMessage = "La description du motif est requise")]
        [StringLength(500, ErrorMessage = "La description ne peut pas dépasser 500 caractères")]
        [Display(Name = "Description du Motif")]
        public string Description { get; set; }

        [Display(Name = "Date de Création")]
        public DateTime DateCreation { get; set; }

        [Required]
        [Display(Name = "Créé par")]
        public string CreatedBy { get; set; }

        [Display(Name = "Statut")]
        public MotifStatus Status { get; set; }

        [Display(Name = "Date de Lecture")]
        public DateTime? DateLecture { get; set; }

        [Display(Name = "Lu par")]
        public string? ReadBy { get; set; } // Made nullable
    }

    public enum MotifStatus
    {
        NonLu = 0,
        Lu = 1,
        Traite = 2
    }
}
