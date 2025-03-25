using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QueueTriggerDemo;

var host = new HostBuilder()
     .ConfigureAppConfiguration((context, config) =>
     {
         // Load local settings for local development (if available)
         config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);

         // Load the base configuration file
         config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

         // Determine the environment and load the corresponding file
         var env = System.Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") ?? "Production";
         config.AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true);
     })
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        // Bind the "MySettings" section of your configuration to your MySettings class
        services.Configure<MySettings>(context.Configuration.GetSection("MySettings"));
    })
    .Build();

host.Run();