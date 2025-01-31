using RestSdk;

namespace ClientApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            HttpClient httpClient = new HttpClient();

            RestClient restClient = new RestClient("https://localhost:9112/", httpClient);

            var result = await restClient.GetAllWeatherForecastsAsync();

            foreach (var item in result)
            {
                Console.WriteLine(item.Value.Summary);
            }
        }
    }
}