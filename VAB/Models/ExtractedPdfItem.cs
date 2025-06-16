using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VAB.Models
{
    public class ExtractedPdfItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ExtractedPdfDataId { get; set; }

        [Required]
        [StringLength(100)]
        public string ItemCode { get; set; } = string.Empty;

        [StringLength(200)]
        public string ItemName { get; set; } = string.Empty;

        [StringLength(500)]
        public string ItemDescription { get; set; } = string.Empty;

        public int RowNumber { get; set; }

        public int ColumnNumber { get; set; }

        [StringLength(1000)]
        public string RawData { get; set; } = string.Empty;

        [StringLength(100)]
        public string Category { get; set; } = "Général";

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Quantity { get; set; } = 0;

        // Navigation property
        [ForeignKey("ExtractedPdfDataId")]
        public virtual ExtractedPdfData ExtractedPdfData { get; set; }
    }
}
