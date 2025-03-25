using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;


namespace ContinuousWebJob
{
    // To learn more about Microsoft Azure WebJobs SDK, please see https://go.microsoft.com/fwlink/?LinkID=320976
    internal class Program
    {
        // Please set the following connection strings in app.config, environment variables,
        // or user secrets for this WebJob to run: AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            var builder = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .AddEnvironmentVariables();
            var configuration = builder.Build();

            JobHostConfiguration jobHostConfiguration = null;

            // If the connection strings are not set, use the settings from app.config.
            // This check is necessary because the Azure WebJobs SDK
            // sets a flag indicating that the corresponding setting has been set and if set to null
            // will throw an exception when the job is run.
            if (string.IsNullOrEmpty(configuration["AzureWebJobsStorage"]) ||
                string.IsNullOrEmpty(configuration["AzureWebJobsDashboard"]))
            {
                jobHostConfiguration = new JobHostConfiguration();
            }
            else
            {
                jobHostConfiguration = new JobHostConfiguration()
                {
                    StorageConnectionString = configuration["AzureWebJobsStorage"],
                    DashboardConnectionString = configuration["AzureWebJobsDashboard"]
                };
            }

            if (jobHostConfiguration.IsDevelopment)
            {
                jobHostConfiguration.UseDevelopmentSettings();
            }

            var host = new JobHost(jobHostConfiguration);
            // The following code ensures that the WebJob will be running continuously.
            host.RunAndBlock();
        }
    }
}
