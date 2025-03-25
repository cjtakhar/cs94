using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSCIE94_CronWebJob
{
    // To learn more about Microsoft Azure WebJobs SDK, please see https://go.microsoft.com/fwlink/?LinkID=320976
    /// <summary>
    /// Main class for the app
    /// </summary>
    // Don't forget to make Program Public!
    public class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            var config = new JobHostConfiguration();

            if (config.IsDevelopment)
            {
                config.UseDevelopmentSettings();
            }

            var host = new JobHost(config);

            // Executes each call once "On Demand"
            host.Call(typeof(Functions).GetMethod("LogSomeData"), new Dictionary<string, object>() { { "message", "I have been CRONNed!" } });

            // Disabled for Triggered jobs / On Demand
            // The following code ensures that the WebJob will
            // be running continuously.
            // Disabling host.RunAndBlock() causes the job to execute once, when triggered, then exit.
            //host.RunAndBlock();
        }
    }
}
