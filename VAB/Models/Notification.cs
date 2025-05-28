namespace VAB.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }

    }
}
