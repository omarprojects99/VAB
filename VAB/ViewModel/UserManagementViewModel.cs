using System.ComponentModel.DataAnnotations;

namespace VAB.ViewModel
{
    public class UserManagementViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public string NomPrenom { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "Phone number must be exactly 8 digits")]
        public string Telephone { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public string Role { get; set; } = string.Empty;

        public List<string>? Roles { get; set; }

        public List<string>? AvailableRoles { get; set; }
    }
}
