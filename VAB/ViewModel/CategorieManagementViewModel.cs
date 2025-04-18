using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using VAB.Models;

namespace VAB.ViewModel
{
    public class CategorieManagementViewModel
       
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; }
    }
}
