namespace VAB.ViewModel
{
    public class UserRoleViewModel
    {

        public string NomPrenom { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}
