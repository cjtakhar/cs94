using System.ComponentModel.DataAnnotations;
namespace WeatherForecastTesting
{
    /// <summary>
    /// Describes the weather forecast information for creation 
    /// </summary>
    public class WeatherForecastCreate
    {
        /// <summary>
        /// The date of the weather forecast
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// The temperature in Celsius 
        /// </summary>
        public int TemperatureC { get; set; }

        /// <summary>
        /// Summary information about the weather forecast 
        /// </summary>
        [MinLength(length: 1)]
        [MaxLength(length: 60)]
        public string? Summary { get; set; }
    }
}