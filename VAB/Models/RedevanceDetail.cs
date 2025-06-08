using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace VAB.Models
{
    public class RedevanceDetail
    {
        public int Id { get; set; }

        public int RedevanceId { get; set; }
        public Redevance Redevance { get; set; }

        public int EtatDesVentesArriveeId { get; set; }
        public EtatDesVentesArrivee EtatDesVentesArrivee { get; set; }

        [Precision(18, 2)]
        public decimal TotalVentes { get; set; }

        [Precision(18, 2)]
        public decimal MontantRedevance { get; set; }

        public string NumeroEtat { get; set; }
        public DateTime DateEtat { get; set; }
        public int NombreArticles { get; set; }
    }
}
