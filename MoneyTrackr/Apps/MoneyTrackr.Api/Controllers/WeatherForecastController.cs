using Microsoft.AspNetCore.Mvc;
using MoneyTrackr.Api.Models;

namespace MoneyTrackr.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            return new WeatherForecast[]
            {
                new()
                {
                  Date = DateTime.Today,
                  TemperatureC = 1,
                  Summary = "Freezing"
                },
                new()
                {
                  Date = DateTime.Today.AddDays(1),
                  TemperatureC = 14,
                  Summary = "Bracing"
                },
                new()
                {
                  Date = DateTime.Today.AddDays(2),
                  TemperatureC = -13,
                  Summary = "Freezing"
                },
                new()
                {
                  Date = DateTime.Today.AddDays(3),
                  TemperatureC = -16,
                  Summary = "Balmy"
                },
                new()
                {
                  Date = DateTime.Today.AddDays(4),
                  TemperatureC = -2,
                  Summary = "Chilly"
                },
            };
        }
    }
}
