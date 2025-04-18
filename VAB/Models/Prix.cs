using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace VAB.Models
{
    public class Prix
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Precision(18, 2)]
        public decimal Valeur { get; set; }

        [Required]

        public DateTime DateDebut { get; set; }

        public DateTime? DateFin { get; set; }
        public int ArticleId { get; set; } // Foreign key
        public Article Article { get; set; } // Navigation




    }
}
