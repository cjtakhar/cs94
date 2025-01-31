using CommonLib;
using Microsoft.AspNetCore.Mvc;

namespace WeatherForecastTesting.Controllers
{
    /// <summary>
    /// Provides fake weather forecast information
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")] // See: https://en.wikipedia.org/wiki/Media_type    
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };

        private readonly ILogger<WeatherForecastController> _logger;
        private static readonly Dictionary<string,WeatherForecast> _weatherForecasts = new Dictionary<string,WeatherForecast>();

        private const string GetWeatherForecastsRouteName = "GetAllWeatherForecasts";
        private const string GetWeatherForecastRouteName = "GetWeatherForecastById";
        private const string CreateWeatherForecastRouteName = "CreateWeatherForecast";
        private const string PatchWeatherForecastRouteName = "PatchWeatherForecastRouteName";

        /// <summary>
        /// Instance constructor 
        /// </summary>
        /// <param name="logger">The logger instance to log to</param>
        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Initialize static data
        /// </summary>
        /// <remarks>
        /// Static constructors are only called once and called before any other method is called
        /// </remarks>
        static WeatherForecastController()
        {
            var randomWeatherForecastsToAdd = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            }).ToArray();
            
            foreach (var weatherForecast in randomWeatherForecastsToAdd)
            {
                _weatherForecasts.Add(Guid.NewGuid().ToString(), weatherForecast);
            }
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
        [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(IEnumerable<WeatherForecastResult>))]
        [HttpGet(Name = GetWeatherForecastsRouteName)]
        public IEnumerable<WeatherForecastResult> GetAllWeatherForecasts()
        {
            return (from weatherForecast in _weatherForecasts
                    select
                    new WeatherForecastResult()
                    {
                        Id = weatherForecast.Key,
                        Date = weatherForecast.Value.Date,
                        Summary = weatherForecast.Value.Summary,
                        TemperatureC = weatherForecast.Value.TemperatureC
                    }).ToArray();
        }

        /// <summary>
        /// Retrieves the weather forecast by id
        /// </summary>
        /// <param name="id">The Id of the weather forecast</param>
        /// <returns>The weather forecast identified by id</returns>        
        /// <response code="200">Indicates the request was successful</response>
        /// <response code="404">Indicates the weather forecast was not found</response>
        [ProducesResponseType(type: typeof(WeatherForecast), statusCode: StatusCodes.Status200OK)]
        [ProducesResponseType(statusCode: StatusCodes.Status404NotFound)]
        [ProducesResponseType(statusCode: StatusCodes.Status500InternalServerError)]
        [HttpGet("{id}",Name = GetWeatherForecastRouteName)]
        public ActionResult<WeatherForecast> GetAWeatherForecastById(string id)
        {
            try
            {
                if (id.Contains("BadRobot"))
                {
                    throw new Exception("Error simulation");
                }

                if (_weatherForecasts.ContainsKey(id))
                {
                    return _weatherForecasts[id];
                }
            }
            catch (Exception ex)
            {
                string causalityId = Guid.NewGuid().ToString();
                _logger.LogCritical(exception:ex,message:"The causality ID is {id}", args:causalityId);
                // Don't return raw exception information to the caller
                // Note: An internally referenceable causality id that could be tied to internally logged information
                //       is recommended so developers can find and resolve the root cause of the issue.
                return StatusCode(StatusCodes.Status500InternalServerError, $"We are sorry experiencing technical difficulties at this time! Provide this number to tech support {causalityId}");
            }
            
            return NotFound();
        }

        [ProducesResponseType(statusCode: StatusCodes.Status204NoContent)]
        [ProducesResponseType(statusCode: StatusCodes.Status404NotFound)]
        [HttpPatch("{id}", Name = PatchWeatherForecastRouteName)]
        public ActionResult Patch(string id, [FromBody] WeatherForecastUpdate weatherForecastUpdate)
        {
            if (!_weatherForecasts.ContainsKey(id))
            {
                return NotFound();
            }

            // Update the entry with values provided, skip update where null
            _weatherForecasts[id].Date = weatherForecastUpdate.Date ?? _weatherForecasts[id].Date;
            _weatherForecasts[id].Summary = weatherForecastUpdate.Summary ?? _weatherForecasts[id].Summary;
            _weatherForecasts[id].TemperatureC = weatherForecastUpdate.TemperatureC ?? _weatherForecasts[id].TemperatureC;

            return NoContent();
        }

        /// <summary>
        /// Creates a new weather forecast entry and adds it to the list of weather forecasts
        /// </summary>
        /// <param name="WeatherForecastCreate">The weather forecast to ad</param>
        /// <response code="201">Indicates the weather forecast was added successfully</response>
        [ProducesResponseType(type:typeof(WeatherForecastResult), statusCode: StatusCodes.Status201Created)]
        [ProducesResponseType(type:typeof(ErrorResponse), statusCode: StatusCodes.Status400BadRequest)]
        [HttpPost(Name = CreateWeatherForecastRouteName)]
        public ActionResult Post([FromBody] WeatherForecastCreate WeatherForecastCreate)
        {
            // Validate input
            if(WeatherForecastCreate == null)
            {
                return BadRequest(new ErrorResponse() 
                { 
                    ErrorMessage = "Input body must not be null", 
                    ErrorNumber = ErrorNumbers.MustNotBeNull,
                    PropertyName = nameof(WeatherForecastCreate)
                });
            }

            if (string.IsNullOrWhiteSpace(WeatherForecastCreate.Summary))
            {
                return BadRequest(new ErrorResponse()
                {
                    ErrorMessage = "Input must not be null",
                    ErrorNumber = ErrorNumbers.MustNotBeNull,
                    PropertyName = nameof(WeatherForecastCreate.Summary)
                });
            }

            WeatherForecast weatherForecast = new WeatherForecast()
            {
                Date = WeatherForecastCreate.Date,
                Summary = WeatherForecastCreate.Summary,
                TemperatureC = WeatherForecastCreate.TemperatureC
            };

            string newKeyAdded = Guid.NewGuid().ToString();
            _weatherForecasts.Add(newKeyAdded, weatherForecast);

            WeatherForecastResult weatherForecastResult = new WeatherForecastResult()
            {
                Id = newKeyAdded,
                Date = WeatherForecastCreate.Date,
                Summary = WeatherForecastCreate.Summary,
                TemperatureC = WeatherForecastCreate.TemperatureC
            };

            return CreatedAtRoute(GetWeatherForecastRouteName,routeValues: new {id=newKeyAdded}, value: weatherForecastResult);
        }
    }
}