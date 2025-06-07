using System.ComponentModel.DataAnnotations;

namespace VAB.ViewModel
{
    public class TrouDeCaisseViewModel
    {
        public int Id { get; set; }

        [Display(Name = "État des Ventes")]
        public int EtatDesVentesArriveeId { get; set; }
        public string NumeroEtat { get; set; }
        public DateTime DateEtat { get; set; }

        [Display(Name = "PNC Responsable")]
        public int PncResponsableId { get; set; }
        public string PncResponsableNom { get; set; }
        public string PncResponsableMatricule { get; set; }

        [Display(Name = "Date de Détection")]
        public DateTime DateDetection { get; set; }

        [Display(Name = "Montant Calculé")]
        [DataType(DataType.Currency)]
        public decimal MontantCalcule { get; set; }

        [Display(Name = "Montant Encaissé")]
        [DataType(DataType.Currency)]
        public decimal MontantEncaisse { get; set; }

        [Display(Name = "Montant Manquant")]
        [DataType(DataType.Currency)]
        public decimal MontantManquant { get; set; }

        [Display(Name = "Motif")]
        public string Motif { get; set; }

        [Display(Name = "Statut")]
        public string Statut { get; set; }

        [Display(Name = "Appliqué à la Commission")]
        public bool EstApplique { get; set; }

        [Display(Name = "Date d'Application")]
        public DateTime? DateApplication { get; set; }

        [Display(Name = "Détecté par")]
        public string DetectePar { get; set; }

        [Display(Name = "Commentaires")]
        public string Commentaires { get; set; }

        // Additional properties for display
        public string EquipageInfo { get; set; }
        public List<ArticleVenteDetailViewModel> ArticlesVendus { get; set; } = new List<ArticleVenteDetailViewModel>();
    }

    public class ArticleVenteDetailViewModel
    {
        public string Description { get; set; }
        public int QuantiteVendu { get; set; }
        public decimal PrixUnitaire { get; set; }
        public decimal Valeur { get; set; }
    }

    public class TrouDeCaisseDashboardViewModel
    {
        [Display(Name = "Trous de Caisse Détectés")]
        public List<TrouDeCaisseSummaryViewModel> TrousDeCaisse { get; set; } = new List<TrouDeCaisseSummaryViewModel>();

        [Display(Name = "Total des Manques")]
        [DataType(DataType.Currency)]
        public decimal TotalManques { get; set; }

        [Display(Name = "Période")]
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }

        public int NombreTrousEnAttente { get; set; }
        public int NombreTrousAppliques { get; set; }
    }

    public class TrouDeCaisseSummaryViewModel
    {
        public int Id { get; set; }
        public string NumeroEtat { get; set; }
        public DateTime DateDetection { get; set; }
        public string PncResponsableNom { get; set; }
        public decimal MontantManquant { get; set; }
        public string Statut { get; set; }
        public bool EstApplique { get; set; }
    }
}
