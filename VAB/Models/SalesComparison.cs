using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VAB.Models
{
   
        public class SalesComparison
        {
            [Key]
            public int Id { get; set; }

            [Required]
            public string Numero { get; set; }

            public DateTime ComparisonDate { get; set; }

            public decimal? TotalEncaisseArrivee { get; set; }
            public decimal? TotalEncaisseDepart { get; set; }
            public bool TotalEncaisseMatch { get; set; }

            public decimal? EcartPlusArrivee { get; set; }
            public decimal? EcartPlusDepart { get; set; }
            public bool EcartPlusMatch { get; set; }

            public decimal? EcartMoinsArrivee { get; set; }
            public decimal? EcartMoinsDepart { get; set; }
            public bool EcartMoinsMatch { get; set; }

            public List<ComparisonArticle> Articles { get; set; } = new List<ComparisonArticle>();
        }

       
    }

