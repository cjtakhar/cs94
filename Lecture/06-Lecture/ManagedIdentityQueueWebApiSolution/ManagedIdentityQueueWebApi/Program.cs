
using Azure.Identity;
using Azure.Storage.Queues;
using ManagedIdentityQueueWebApi.Settings;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace ManagedIdentityQueueWebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                // Add nice title
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Managed Identity Azure Queue Demo", Version = "v1" });

                // Add documentation via C# XML Comments
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                // Note: Don't forget to add this to the csproj file (removing /* and */)
                /*
                    <!-- DEMO: Enable documentation file so API has user defined documentation via XML comments -->
                    <PropertyGroup>
                        <GenerateDocumentationFile>true</GenerateDocumentationFile>

                    <!-- Disable warning: Missing XML comment for publicly visible type or member 'Type_or_Member'-->
                    <NoWarn>$(NoWarn);1591</NoWarn>
                    </PropertyGroup>      
                */
            });

            // Validate QueueSettings to ensure they are not null or empty
            var logger = loggerFactory.CreateLogger("Program");

            // Add the class that represents the settings
            LimitSettings limitSettings = new LimitSettings();
            builder.Configuration.GetSection(nameof(LimitSettings)).Bind(limitSettings);
            builder.Services.AddSingleton(implementationInstance: limitSettings);

            // Add the class that represents the settings for the queue 
            // Bind AISettings from appsettings.json
            QueueSettings? _queueSettings = builder.Configuration.GetSection("QueueSettings").Get<QueueSettings>()!;

            if (_queueSettings is null
                || string.IsNullOrWhiteSpace(_queueSettings.JsonQueueConnectionString)
                || string.IsNullOrWhiteSpace(_queueSettings.StringQueueConnectionString))
            {
                if (_queueSettings is null)
                {
                    logger.LogCritical("QueueSettings is null. Please check the configuration.");
                }
                else if (string.IsNullOrWhiteSpace(_queueSettings.JsonQueueConnectionString))
                {
                    logger.LogCritical("QueueSettings.JsonQueueConnectionString is null or empty. Please check the configuration.");
                }
                else if (string.IsNullOrWhiteSpace(_queueSettings.StringQueueConnectionString))
                {
                    logger.LogCritical("QueueSettings.StringQueueConnectionString is null or empty. Please check the configuration.");
                }

                throw new InvalidOperationException(nameof(_queueSettings));
            }

            // If validation passes, log success
            logger.LogInformation("QueueSettings loaded successfully.");

            builder.Configuration.GetSection(nameof(QueueSettings)).Bind(_queueSettings);
            builder.Services.AddSingleton(implementationInstance: _queueSettings);

            // Add the queue client for a string queue demo
            builder.Services.AddSingleton<StringQueueClient>(sp =>
            {
                // Create a DefaultAzureCredential instance, which will use the managed identity when running in Azure.
                var credential = new DefaultAzureCredential();

                StringQueueClient queueClient = new StringQueueClient(new Uri(_queueSettings.StringQueueConnectionString), credential);

                return queueClient;
            });

            // Add the queue client for a json queue demo
            builder.Services.AddSingleton<JsonQueueClient>(sp =>
            {
                // Create a DefaultAzureCredential instance, which will use the managed identity when running in Azure.
                var credential = new DefaultAzureCredential();

                JsonQueueClient queueClient = new JsonQueueClient(new Uri(_queueSettings.JsonQueueConnectionString), credential);

                return queueClient;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {

            }
            // Code Note: Moved outside of env.IsDevelopment() so both 
            // Debug and Release are supported
            app.UseSwagger();

            // Customize the UseSwaggerUI() 
            app.UseSwaggerUI(c =>
            {
                // 1. Display a friendly title
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");

                // Code Note: 
                // Launch the Swagger UI by default
                // Serving the Swagger UI at the app's root 
                // (http://localhost:<port>)
                c.RoutePrefix = string.Empty;
            });

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
