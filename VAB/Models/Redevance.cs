using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace VAB.Models
{
    public class Redevance
    {
        public int Id { get; set; }

        [Required]
        public int Mois { get; set; }

        [Required]
        public int Annee { get; set; }

        [Precision(18, 2)]
        public decimal TotalVentes { get; set; }

        [Precision(18, 2)]
        public decimal MontantRedevance { get; set; }

        public DateTime DateCalcul { get; set; }

        public int NombreEtats { get; set; }

        public string CalculeParId { get; set; }
        public User CalculePar { get; set; }

        public bool IsValidated { get; set; }

        public List<RedevanceDetail> RedevanceDetails { get; set; } = new List<RedevanceDetail>();

        public string PeriodeDisplay => $"{GetMonthName(Mois)} {Annee}";

        private string GetMonthName(int month)
        {
            return month switch
            {
                1 => "Janvier",
                2 => "Février",
                3 => "Mars",
                4 => "Avril",
                5 => "Mai",
                6 => "Juin",
                7 => "Juillet",
                8 => "Août",
                9 => "Septembre",
                10 => "Octobre",
                11 => "Novembre",
                12 => "Décembre",
                _ => "Inconnu"
            };
        }
    }
}
