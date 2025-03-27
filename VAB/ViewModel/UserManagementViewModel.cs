using System.Data;

namespace VAB.ViewModel
{
    public class UserManagementViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string NomPrenom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telephone { get; set; }   = string.Empty; // Changed to string
        public bool IsActive { get; set; }
        public string Role { get; set; }= string.Empty;
        public List<string>? Roles { get; set; }
        public List<string>? AvailableRoles { get; set; }
    }
}
