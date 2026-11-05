using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace VAB.Services
{
    public interface ITwilioSmsService
    {
        Task<bool> SendSmsAsync(string phoneNumber, string message);
    }

    public class TwilioSmsService : ITwilioSmsService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TwilioSmsService> _logger;

        private readonly string? _accountSid;
        private readonly string? _messagingServiceSid;
        private readonly string? _authToken;

        public TwilioSmsService(HttpClient httpClient, IConfiguration configuration, ILogger<TwilioSmsService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _accountSid = _configuration["Twilio:AccountSid"];
            _messagingServiceSid = _configuration["Twilio:MessagingServiceSid"];
            _authToken = _configuration["Twilio:AuthToken"];
        }

        public async Task<bool> SendSmsAsync(string phoneNumber, string message)
        {
            if (string.IsNullOrWhiteSpace(_accountSid)
                || string.IsNullOrWhiteSpace(_messagingServiceSid)
                || string.IsNullOrWhiteSpace(_authToken))
            {
                _logger.LogError("SMS was not sent because Twilio credentials are not configured.");
                return false;
            }

            try
            {
                var url = $"https://api.twilio.com/2010-04-01/Accounts/{_accountSid}/Messages.json";

                var formData = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("To", phoneNumber),
                    new KeyValuePair<string, string>("MessagingServiceSid", _messagingServiceSid),
                    new KeyValuePair<string, string>("Body", message)
                };

                var formContent = new FormUrlEncodedContent(formData);

                // Add Basic Authentication
                var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_accountSid}:{_authToken}"));
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authValue);

                var response = await _httpClient.PostAsync(url, formContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"SMS sent successfully to {phoneNumber}. Response: {responseContent}");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to send SMS to {phoneNumber}. Status: {response.StatusCode}, Error: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception occurred while sending SMS to {phoneNumber}");
                return false;
            }
        }
    }
}
