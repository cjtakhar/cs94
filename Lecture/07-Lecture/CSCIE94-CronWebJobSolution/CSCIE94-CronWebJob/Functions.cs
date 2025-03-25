using Microsoft.Azure.WebJobs;
using System;
using System.IO;

namespace CSCIE94_CronWebJob
{
    public class Functions
    {
        /// <summary>
        /// This method will get triggered based on a interval based on the settings in the
        /// settings.json file
        /// </summary>
        /// <param name="log">The logger stream</param>     
        [NoAutomaticTrigger]
        public static void LogSomeData(TextWriter logger, string message)
        {
            logger.WriteLine($"The C# Timer trigger function executed at: {DateTime.Now} message: [{message}]");
        }
    }
}
