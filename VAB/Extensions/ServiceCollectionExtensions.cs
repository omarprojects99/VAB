using Microsoft.Extensions.DependencyInjection;
using VAB.Services;

namespace VAB.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTwilioServices(this IServiceCollection services)
        {
            services.AddHttpClient<ITwilioSmsService, TwilioSmsService>();
            return services;
        }
    }
}
