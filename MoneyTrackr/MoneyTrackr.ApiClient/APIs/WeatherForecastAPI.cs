using MoneyTrackr.ApiClient.DTOs;
using System.Net.Http.Json;

namespace MoneyTrackr.ApiClient.APIs
{
    public interface IWeatherForecastAPI
    {
        Task<IList<WeatherForecastDto>?> GetForecastAsync();
    }

    public class WeatherForecastAPI : IWeatherForecastAPI
    {
        private readonly HttpClient _httpClient;

        public WeatherForecastAPI(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<IList<WeatherForecastDto>?> GetForecastAsync()
        {
            return await _httpClient.GetFromJsonAsync<IList<WeatherForecastDto>>("weatherforecast");
        }
    }
}
