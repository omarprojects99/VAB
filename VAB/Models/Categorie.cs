using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace VAB.Models
{
    public class Categorie
    {
        public int Id { get; set; }

        [Required]
        public string Description { get; set; }

        public ICollection<Article> Articles { get; set; }

        

        

        
    }
}
