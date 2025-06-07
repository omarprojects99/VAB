namespace VAB.Models
{
    public class ExchangeRate
    {
        public int Id { get; set; }
        public decimal EuroToDinar { get; set; }
        public DateTime LastUpdated { get; set; }  // To track when it was last updated
    }
}
