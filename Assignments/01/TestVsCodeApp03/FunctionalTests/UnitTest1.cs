namespace FunctionalTests;

public class FunctionalTests
{

    const string EndpointUrlString = "https://localhost:7027/";

    [Fact]
    public async Task PositiveTest_RetrieveAllWeatherForecasts()
    {

        // Arrange         
        using HttpClient httpClient = new HttpClient();
        WeatherForecastRestClient weatherForecastRestClient = new WeatherForecastRestClient(EndpointUrlString, httpClient);

        // Act - Make the HTTP GET call
        var weatherForecastResults = await weatherForecastRestClient.GetWeatherForecastAsync();

        // Assert a result is returned
        Assert.NotNull(weatherForecastResults);

        // Assert - Each entry has a summary
        int index = 0;
        foreach (var item in weatherForecastResults)
        {
            index++;
            Assert.False(string.IsNullOrWhiteSpace(item.Summary));
        }
    }
}