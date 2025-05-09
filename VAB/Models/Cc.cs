using System.ComponentModel.DataAnnotations;

namespace VAB.Models
{
    public class Cc
    {
        [Key]
        public int CcId { get; set; }

        [Required]
        [StringLength(100)]
        public string Nom { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Prenom { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Matricule { get; set; } = string.Empty;

        [StringLength(50)]
        public string Base { get; set; } = string.Empty;

        [StringLength(50)]
        public string College { get; set; } = string.Empty;

        [StringLength(50)]
        public string Secteur { get; set; } = string.Empty;
    }
}