namespace WeatherForecastTesting
{
    /// <summary>
    /// Describes the weather forecast information
    /// </summary>
    public class WeatherForecast
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
        /// The temperature in Fahrenheit 
        /// </summary>
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        /// <summary>
        /// The summary of the weather forecast 
        /// </summary>
        public string? Summary { get; set; }
    }
}