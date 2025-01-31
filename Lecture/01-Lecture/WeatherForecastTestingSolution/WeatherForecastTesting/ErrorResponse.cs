namespace WeatherForecastTesting
{
    /// <summary>
    /// Describe the error response 
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// The error number to be used programmatically 
        /// </summary>
        public int ErrorNumber { get; set; }

        /// <summary>
        /// A human readable description of the error for developer to read 
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// The name of the property involved in the air  
        /// </summary>
        public string? PropertyName { get; set; }
    }
}
