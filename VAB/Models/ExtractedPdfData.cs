using System.ComponentModel.DataAnnotations;

namespace VAB.Models
{
    public class ExtractedPdfData
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le numéro est requis.")]
        [StringLength(100)]
        public string Numero { get; set; }

        [Required(ErrorMessage = "Le nom du fichier est requis.")]
        [StringLength(255)]
        public string FileName { get; set; }

        [Required(ErrorMessage = "La date d'extraction est requise.")]
        [DataType(DataType.DateTime)]
        public DateTime DateExtraction { get; set; }

        [Required]
        public string JsonData { get; set; } // Store the raw JSON data

        [StringLength(500)]
        public string Description { get; set; }

        public bool IsProcessed { get; set; } = false;

        [StringLength(100)]
        public string ExtractedBy { get; set; } // User who extracted

        public DateTime? ProcessedDate { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }

        // Navigation property for related extracted items
        public List<ExtractedPdfItem> ExtractedItems { get; set; } = new List<ExtractedPdfItem>();
    }
}