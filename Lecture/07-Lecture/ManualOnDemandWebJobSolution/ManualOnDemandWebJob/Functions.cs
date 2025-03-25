using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace ManualOnDemandWebJob
{
    public class Functions
    {
        private const int _pollingIntervalMilliseconds = 2000;
        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called queue.
        [NoAutomaticTrigger]
        public static async Task ProcessQueueMessageOnDemand(TextWriter log)
        {
            CloudQueue queue = await GetCloudQueue();

            CloudQueueMessage message;

            int msgCount = 0;

            do
            {
                // Retrieve the message
                message = queue.GetMessage();

                if (message != null)
                {
                    msgCount++;
                    log.WriteLine($"{DateTime.UtcNow.ToString("u")} Message: [{message.AsString}]");

                    // Remove the message from the queue
                    queue.DeleteMessage(message);
                }
                else
                {
                    if (msgCount > 0)
                    {
                        log.WriteLine($"{DateTime.UtcNow.ToString("u")} Found {msgCount} messages. No more messages found");
                    }
                    else
                    {
                        log.WriteLine($"{DateTime.UtcNow.ToString("u")} No message found");
                    }
                }

                log.WriteLine($"{DateTime.UtcNow.ToString("u")} Pausing for {_pollingIntervalMilliseconds / 1000} seconds before polling for more messages");
                await Task.Delay(_pollingIntervalMilliseconds);
            } while (message != null);

            log.WriteLine($"{DateTime.UtcNow.ToString("u")} Exiting Web Job");
            await Task.Delay(_pollingIntervalMilliseconds);
            log.WriteLine($"{DateTime.UtcNow.ToString("u")} Web Job execution terminated");
        }


        private static async Task<CloudQueue> GetCloudQueue()
        {
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString);

            // Create the queue client.
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            // Retrieve a reference to a queue.
            CloudQueue queue = queueClient.GetQueueReference("mwjqueue");

            // Create the queue if it doesn't already exist.
            await queue.CreateIfNotExistsAsync();

            return queue;

        }
    }
}
