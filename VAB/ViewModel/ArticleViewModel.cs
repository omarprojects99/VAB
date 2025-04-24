using System.ComponentModel.DataAnnotations;

namespace VAB.ViewModel
{
    public class ArticleViewModel
    {
        [Required(ErrorMessage = "Article ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Article ID must be a positive number")]
        public int IdArticle { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Category ID is required")]
        public int CategorieId { get; set; }
    }
}
