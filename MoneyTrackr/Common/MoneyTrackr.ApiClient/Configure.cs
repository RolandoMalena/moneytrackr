using Microsoft.Extensions.DependencyInjection;
using MoneyTrackr.ApiClient.APIs;

namespace MoneyTrackr.ApiClient
{
    public static class Configure
    {
        public static IServiceCollection SetupMoneyTrackrClient(this IServiceCollection services, ClientConfigurationSettings settings)
        {
            if (settings is null)
                throw new ArgumentNullException(nameof(settings));

            if (settings.BaseApiAddress is null)
                throw new ArgumentNullException($"{nameof(settings)}.{nameof(settings.BaseApiAddress)}");

            services.SetupClient<IWeatherForecastAPI, WeatherForecastAPI>(settings);

            return services;
        }

        private static IServiceCollection SetupClient<TClient, TImplementation>(this IServiceCollection services, ClientConfigurationSettings settings) where TClient : class where TImplementation : class, TClient
        {
            services.AddHttpClient<TClient, TImplementation>("MoneyTrackrClient", client =>
            {
                client.BaseAddress = settings.BaseApiAddress;
                client.Timeout = new TimeSpan(0, 0, 30);
                client.DefaultRequestHeaders.Clear();
            });
            return services;
        }
    }
}
