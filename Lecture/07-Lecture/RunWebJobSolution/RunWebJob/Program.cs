using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;

namespace RunWebJob
{
    internal class Program
    {

        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            builder.AddUserSecrets<Program>();
            var config = builder.Build();

            string userName = config["UserName"];
            string password = config["Password"];

            HttpClient httpClient = new HttpClient();
            string encodedCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{userName}:{password}"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encodedCredentials);
            httpClient.BaseAddress = new Uri("https://app-manualondemandwebjob.scm.azurewebsites.net/api/triggeredwebjobs/ManualOnDemandWebJob/run");
            HttpResponseMessage response = httpClient.PostAsync("", null).Result;
            Console.WriteLine(response);
        }
    }
}
