using Microsoft.Extensions.DependencyInjection;
using MoneyTrackr.ApiClient.APIs;

namespace MoneyTrackr.ApiClient
{
    public static class Configure
    {
        private const string EnvironmentVariableName = "API_URL";
        private const string LocalBaseAddress = "https://localhost:44356";

        public static IServiceCollection AddMoneyTrackrClient(this IServiceCollection services)
        {
            services.AddClient<IWeatherForecastAPI, WeatherForecastAPI>();

            return services;
        }

        private static IServiceCollection AddClient<TClient, TImplementation>(this IServiceCollection services) where TClient : class where TImplementation : class, TClient
        {
            string baseAddress = Environment.GetEnvironmentVariable(EnvironmentVariableName) ?? LocalBaseAddress;

            services.AddHttpClient<TClient, TImplementation>("MoneyTrackrClient", client =>
            {
                client.BaseAddress = new Uri(baseAddress);
                client.Timeout = new TimeSpan(0, 0, 30);
                client.DefaultRequestHeaders.Clear();
            });
            return services;
        }
    }
}
