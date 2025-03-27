using Microsoft.AspNetCore.Identity;

namespace VAB.Models
{
    public class User : IdentityUser
    {
        public string NomPrenom { get; set; } = string.Empty;
        public string Telephone { get; set; } =string.Empty;
        //public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
