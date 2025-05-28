using System.ComponentModel.DataAnnotations;

namespace VAB.Models
{
    public class EtatDesOffresDepart
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le numéro est requis.")]
        public string Numero { get; set; }

        [Required(ErrorMessage = "La date d'édition est requise.")]
        [DataType(DataType.DateTime)]
        public DateTime DateEdition { get; set; }

        public bool IsValidated { get; set; }

        public List<ArticleOffreDepart> ArticleOffreDepart { get; set; } = new List<ArticleOffreDepart>();
    }
}