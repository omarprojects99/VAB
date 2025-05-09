namespace VAB.Models
{
    public class FeuilleEtatDesVentesArrivee
    {
        public int FeuilleDeLignesId { get; set; }
        public FeuilleDeLignes FeuilleDeLignes { get; set; }

        public int EtatDesVentesArriveeId { get; set; }
        public EtatDesVentesArrivee EtatDesVentesArrivee { get; set; }

    }
}
