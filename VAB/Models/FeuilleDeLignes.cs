using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VAB.Models
{
    public class FeuilleDeLignes
    {
        public int Id { get; set; }

        [Required]
        public int Numero { get; set; }  

        [Required]
        public int Mois { get; set; } 

        [Required]
        public int Annee { get; set; }

        public DateTime DateGeneration { get; set; } = DateTime.Now;

        //liste des vols
        public List<Vol> Vols { get; set; } = new List<Vol>();

        // Many-to-many with EtatDesVentesArrivee
        public List<FeuilleEtatDesVentesArrivee> FeuilleEtatDesVentes { get; set; }

    }
}
