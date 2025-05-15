namespace VAB.Models
{
    public class FeuilleEtatDesVentesDepart
    {
        public int FeuilleDeLignesId { get; set; }
        public FeuilleDeLignes FeuilleDeLignes { get; set; }

        public int EtatDesVentesDepartId { get; set; }
        public EtatDesVentesDepart EtatDesVentesDepart { get; set; }
    }
}
