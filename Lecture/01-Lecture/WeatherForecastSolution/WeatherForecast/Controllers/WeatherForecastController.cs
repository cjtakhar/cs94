using Microsoft.AspNetCore.Mvc;

namespace WeatherForecast.Controllers
{
    /// <summary>
    /// Provides fake weather forecast information
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        /// <summary>
        /// Instance constructor 
        /// </summary>
        /// <param name="logger">The logger instance to log to</param>
        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Provides a randomly generated set of weather forecasts
        /// </summary>
        /// <returns>A list of weather forecasts</returns>
        /// <remarks>
        /// Sample request:
        /// 
        /// GET /weatherforecast
        /// 
        /// </remarks>
        /// <response code="200">Indicates the request was successful</response>
        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
