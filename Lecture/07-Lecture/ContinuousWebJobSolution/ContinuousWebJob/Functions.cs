using Microsoft.Azure.WebJobs;
using System;
using System.IO;

namespace ContinuousWebJob
{
    public class Functions
    {
        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called queue.
        public static void ProcessQueueMessage([QueueTrigger("wjqueue")] string message, TextWriter log)
        {
            log.WriteLine($"{DateTime.UtcNow.ToString("u")} Message: [{message}]");
            Console.WriteLine($"{DateTime.UtcNow.ToString("u")} Message: [{message}]");
        }
    }
}
