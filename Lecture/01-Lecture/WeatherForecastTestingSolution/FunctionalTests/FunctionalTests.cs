using Xunit;
using WeatherForecastRest;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Http;
using CommonLib;
using Newtonsoft.Json;
using Xunit.Abstractions;

namespace FunctionalTests
{
    public class FunctionalTests
    {

        private readonly ITestOutputHelper _outputHelper;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="outputHelper">Output helper for debugging</param>
        public FunctionalTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _outputHelper.WriteLine("New Instance!");
        }

        // DEMO: Local testing
        const string EndpointUrlString = "https://localhost:9112/";


        // DEMO: Testing against azure deployed Web API App
        //const string EndpointUrlString = "https://app-lecture01-weatherforecast-test-linux-cscie94.azurewebsites.net/";

        /// <summary>
        /// Test retrieving all weather forecasts
        /// </summary>
        /// <returns>Task</returns>
        [Fact]
        public async Task PositiveTest_RetrieveAllWeatherForecasts()
        {
            _outputHelper.WriteLine($"The EndpointUrlString is: [{EndpointUrlString}]");

            // Arrange         
            using HttpClient httpClient = new HttpClient();
            WeatherForecastRestClient weatherForecastRestClient = new WeatherForecastRestClient(EndpointUrlString, httpClient);

            //�Act - Make the HTTP GET call
            var  weatherForecastResults = await weatherForecastRestClient.GetAllWeatherForecastsAsync();

            // Assert a result is returned
            Assert.NotNull(weatherForecastResults);

            _outputHelper.WriteLine($"The number of weather forecasts is: [{weatherForecastResults.Count}]");

            // Assert - Each entry has a summary
            int index = 0;
            foreach (var item in weatherForecastResults)
            {
                index++;
                Assert.False(string.IsNullOrWhiteSpace(item.Summary));
            }
        }

        /// <summary>
        /// Test creating a weather forecast
        /// </summary>
        /// <returns>Task</returns>
        [Fact]
        public async Task PositiveTest_CreateWeatherForecast()
        {
            // Arrange
            using HttpClient httpClient = new HttpClient();
            WeatherForecastRestClient weatherForecastRestClient = new WeatherForecastRestClient(EndpointUrlString, httpClient);
            WeatherForecastCreate weatherForecastInput = new WeatherForecastCreate()
            {
                Date = DateTime.UtcNow,
                Summary = "Test add a weather forecast",
                TemperatureC = 42
            };

            //�Act - Make the HTTP POST call
            WeatherForecastResult? result = await weatherForecastRestClient.CreateWeatherForecastAsync(weatherForecastInput);

            // Assert a result is returned
            Assert.NotNull(result);

            // Verify the item returned matches the input
            Assert.Equal(StatusCodes.Status201Created, weatherForecastRestClient.LastStatusCode);
            Assert.Equal(expected: weatherForecastInput.Summary, result.Summary);
            Assert.Equal(expected: weatherForecastInput.TemperatureC, result.TemperatureC);
            Assert.Equal(expected: weatherForecastInput.Date, result.Date);
        }

        /// <summary>
        /// Test update a weather forecast
        /// </summary>
        /// <returns>Task</returns>
        [Fact]
        public async Task PositiveTest_UpdateWeatherForecast()
        {
            // Arrange
            using HttpClient httpClient = new HttpClient();
            WeatherForecastRestClient weatherForecastRestClient = new WeatherForecastRestClient(EndpointUrlString, httpClient);
            WeatherForecastCreate weatherForecastInput = new WeatherForecastCreate()
            {
                Date = DateTime.UtcNow,
                Summary = "Test add a weather forecast",
                TemperatureC = 42
            };
            
            // Add the weather forecast entry
            WeatherForecastResult? result = await weatherForecastRestClient.CreateWeatherForecastAsync(weatherForecastInput);
            WeatherForecastUpdate weatherForecastUpdate = new WeatherForecastUpdate()
            {
                Summary = "Test update a weather forecast"
            };


            // Act - Update the entry
            await weatherForecastRestClient.PatchWeatherForecastRouteNameAsync(result.Id, weatherForecastUpdate);
            
            // Assert result are as expected
            Assert.Equal(StatusCodes.Status204NoContent, weatherForecastRestClient.LastStatusCode);

            // Retrieve the updated entry
            WeatherForecast updatedResult = await weatherForecastRestClient.GetWeatherForecastByIdAsync(result.Id);

            // Verify the item returned matches the input
            Assert.Equal(expected: weatherForecastUpdate.Summary, updatedResult.Summary);
            Assert.Equal(expected: weatherForecastInput.TemperatureC, updatedResult.TemperatureC);
            Assert.Equal(expected: weatherForecastInput.Date, updatedResult.Date);
        }

        /// <summary>
        /// Test creating a weather forecast
        /// </summary>
        /// <returns>Task</returns>
        [Theory]
        [InlineData(1)] // Empty summary
        [InlineData(60)] // To large summary
        public async Task PositiveTest_CreateWeatherForecast_MinMaxSummary(int summaryLength)
        {
            // Arrange
            using HttpClient httpClient = new HttpClient();
            WeatherForecastRestClient weatherForecastRestClient = new WeatherForecastRestClient(EndpointUrlString, httpClient);
            WeatherForecastCreate weatherForecastInput = new WeatherForecastCreate()
            {
                Date = DateTime.UtcNow,
                Summary = summaryLength < 0 ? null : new string(c: 'X', summaryLength),
                TemperatureC = 42
            };

            //�Act - Make the HTTP POST call
            WeatherForecastResult? result = await weatherForecastRestClient.CreateWeatherForecastAsync(weatherForecastInput);

            // Verify the item returned matches the input
            Assert.Equal(expected: weatherForecastInput.Summary, result.Summary);
            Assert.Equal(expected: weatherForecastInput.TemperatureC, result.TemperatureC);
            Assert.Equal(expected: weatherForecastInput.Date, result.Date);

            // Verify the location header is not null
            Assert.False(string.IsNullOrWhiteSpace(weatherForecastRestClient.LastLocationHeader));

            // Retrieve the weather forecast using the location header
            var locationHeaderResponse = await httpClient.GetAsync(weatherForecastRestClient.LastLocationHeader);

            // Verify the data was returned using location header url successfully
            Assert.Equal(StatusCodes.Status200OK, (int)locationHeaderResponse.StatusCode);

            // Get the content of the response returned from the request to the location header url
            string locationHeaderResponseString = await locationHeaderResponse.Content.ReadAsStringAsync();

            // Verify that a response was returned
            Assert.False(string.IsNullOrWhiteSpace(locationHeaderResponseString));

            // Verify that the response is the correct type
            WeatherForecast? locationHeaderResponseDeserialized = JsonConvert.DeserializeObject<WeatherForecast>(locationHeaderResponseString);
            Assert.NotNull(locationHeaderResponseDeserialized);
            
            // Verify the item returned matches the input
            Assert.Equal(expected: weatherForecastInput.Summary, locationHeaderResponseDeserialized?.Summary);
            Assert.Equal(expected: weatherForecastInput.TemperatureC, locationHeaderResponseDeserialized?.TemperatureC);
            Assert.Equal(expected: weatherForecastInput.Date, locationHeaderResponseDeserialized?.Date);            
        }

        /// <summary>
        /// Test creating a weather forecast verify the location header
        /// </summary>
        /// <returns>Task</returns>
        [Fact]
        public async Task PositiveTest_CreateWeatherForecast_LocationHeader()
        {
            // Arrange
            using HttpClient httpClient = new HttpClient();
            WeatherForecastRestClient weatherForecastRestClient = new WeatherForecastRestClient(EndpointUrlString, httpClient);
            WeatherForecastCreate weatherForecastInput = new WeatherForecastCreate()
            {
                Date = DateTime.UtcNow,
                Summary = new string(c: 'X', 60),
                TemperatureC = 41
            };

            //�Act - Make the HTTP POST call
            WeatherForecastResult? result = await weatherForecastRestClient.CreateWeatherForecastAsync(weatherForecastInput);

            // Retrieve the weather forecast using the location header
            var locationHeaderResponse = await httpClient.GetAsync(weatherForecastRestClient.LastLocationHeader);

            // Get the content of the response returned from the request to the location header url
            string locationHeaderResponseString = await locationHeaderResponse.Content.ReadAsStringAsync();

            // Assert - Verify the response based on the location header

            // Assert that a response was returned
            Assert.False(string.IsNullOrWhiteSpace(locationHeaderResponseString));

            // Assert that the response is the correct type
            WeatherForecast? locationHeaderResponseDeserialized = JsonConvert.DeserializeObject<WeatherForecast>(locationHeaderResponseString);
            Assert.NotNull(locationHeaderResponseDeserialized);

            // Assert the item returned matches the input
            Assert.Equal(expected: weatherForecastInput.Summary, locationHeaderResponseDeserialized?.Summary);
            Assert.Equal(expected: weatherForecastInput.TemperatureC, locationHeaderResponseDeserialized?.TemperatureC);
            Assert.Equal(expected: weatherForecastInput.Date, locationHeaderResponseDeserialized?.Date);
        }

        /// <summary>
        /// Test creating a weather forecast
        /// </summary>
        /// <returns>Task</returns>
        [Theory]
        [InlineData(-1)] // Null summary
        [InlineData(0)] // Empty summary
        [InlineData(61)] // To large summary
        public async Task NegativeTest_CreateWeatherForecast_InvalidSummary(int summaryLength)
        {
            // Arrange
            using HttpClient httpClient = new HttpClient();
            WeatherForecastRestClient weatherForecastRestClient = new WeatherForecastRestClient(EndpointUrlString, httpClient);
            WeatherForecastCreate weatherForecastInput = new WeatherForecastCreate()
            {
                Date = DateTime.UtcNow,
                Summary = summaryLength < 0 ? null : new string(c: 'X', summaryLength),
                TemperatureC = 42
            };

            //�Act - Make the HTTP POST call
            ApiException apiException = await Assert.ThrowsAsync<ApiException<ErrorResponse>>(() => weatherForecastRestClient.CreateWeatherForecastAsync(weatherForecastInput));
            
            // Assert 
            Assert.Equal(expected: StatusCodes.Status400BadRequest, actual: apiException.StatusCode);
        }

        /// <summary>
        /// Test creating a weather forecast
        /// </summary>
        /// <returns>Task</returns>
        [Fact]
        public async Task NegativeTest_reateWeatherForecast_NullSummary()
        {
            try
            {
                // Arrange
                using HttpClient httpClient = new HttpClient();
                WeatherForecastRestClient weatherForecastRestClient = new WeatherForecastRestClient(EndpointUrlString, httpClient);
                WeatherForecastCreate weatherForecastInput = new WeatherForecastCreate()
                { Date = DateTime.UtcNow, Summary = null, TemperatureC = 31 };

                //�Act - Make the HTTP POST call
                WeatherForecastResult? result = await weatherForecastRestClient.CreateWeatherForecastAsync(weatherForecastInput);

            }
            catch (ApiException<ErrorResponse> apiEx)
            {
                Assert.Equal(StatusCodes.Status400BadRequest, apiEx.StatusCode);
                Assert.Equal(ErrorNumbers.MustNotBeNull, apiEx.Result.ErrorNumber);
                Assert.Equal(nameof(WeatherForecastCreate.Summary), apiEx.Result.PropertyName);
            }
        }

    }
}