using System.ComponentModel.DataAnnotations;

namespace VAB.ViewModel;

public class ExchangeRateViewModel
{
    [Required]
    [Range(0.01, 1000, ErrorMessage = "Please enter a valid exchange rate.")]
    [Display(Name = "Euro to Dinar Exchange Rate")]
    public decimal EuroToDinar { get; set; }
    public decimal? CurrentApiRate { get; set; } // For API-fetched rate

}
