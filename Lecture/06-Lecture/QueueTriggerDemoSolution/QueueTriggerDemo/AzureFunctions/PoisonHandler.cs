using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using Azure.Storage.Blobs;
using Azure.Storage.Queues.Models;

namespace QueueTriggerDemo.AzureFunctions
{
    public class PoisonHandler
    {
        private readonly ILogger<PoisonHandler> _logger;
        private readonly BlobServiceClient _blobServiceClient;

        public PoisonHandler(ILogger<PoisonHandler> logger,
            BlobServiceClient blobServiceClient)
        {
            _logger = logger;
            _blobServiceClient = blobServiceClient;
        }

        [Function("PoisonHandler")]
        public void Run([QueueTrigger("myqueue-items-function-poison", Connection = "StorageConnections")] QueueMessage queueMessage)
        {
            _logger.LogCritical($"PoisonHandler: C# Queue trigger function processed message: {queueMessage.MessageText}");

            var containerName = "poisonmessages";
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            if (!blobContainerClient.Exists())
            {
                _logger.LogInformation($"Container '{containerName}' does not exist. Creating container...");
                blobContainerClient.Create();
                _logger.LogInformation($"Container '{containerName}' created successfully.");
            }
            else
            {
                _logger.LogInformation($"Container '{containerName}' already exists.");
            }

            var blobName = $"{queueMessage.MessageId}.txt";
            var blobClient = blobContainerClient.GetBlobClient(blobName);
            if (!blobClient.Exists())
            {
                _logger.LogInformation($"Blob '{blobName}' does not exist. Uploading message...");
                using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(queueMessage.MessageText)))
                {
                    blobClient.Upload(stream, overwrite: true);
                    _logger.LogInformation($"Blob '{blobName}' uploaded successfully.");
                }
            }
            else
            {
                _logger.LogInformation($"Blob '{blobName}' already exists. Skipping upload.");
            }
        }
    }
}
