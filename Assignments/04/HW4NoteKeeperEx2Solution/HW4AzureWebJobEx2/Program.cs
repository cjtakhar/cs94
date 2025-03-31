// Program.cs
using System;
using System.IO;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Blobs;
using Azure.Data.Tables;
using Newtonsoft.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using System.IO.Compression;

namespace HW4AzureWebJobEx2
{
    /// <summary>
    /// Entry point for the WebJob.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Main method to initialize and run the WebJob.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                // Configure Azure WebJobs with default settings
                .ConfigureWebJobs(b =>
                {
                    b.AddAzureStorage();
                })
                .Build();

            // Run the WebJob
            using (host)
            {
                await host.RunAsync();
            }
        }
    }

    /// <summary>
    /// Defines the functions that will be triggered by Azure Storage Queues.
    /// </summary>
    public class Functions
    {
        /// <summary>
        /// Processes messages from the attachment-zip-requests-wj-ex2 queue.
        /// </summary>
        /// <param name="message">Queue message in JSON format containing a zip request.</param>
        public static async Task ProcessQueueMessage(
            [QueueTrigger("attachment-zip-requests-wj-ex2")] string message)
        {
            Console.WriteLine($"Processing message: {message}");

            // Deserialize the queue message into a ZipRequest object
            var request = JsonConvert.DeserializeObject<ZipRequest>(message);
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Invalid queue message.");
            }

            string containerName = request.NoteId + "-zip";

            try
            {
                // Create zip from attachments
                var zipFilePath = await ZipService.CreateZipFromAttachments(request.NoteId!);

                // Upload zip to blob storage
                await BlobStorageService.UploadZipToBlob(containerName, request.ZipFileId!, zipFilePath);

                // Update job status in Table Storage
                await TableStorageService.UpdateJobStatus(request.NoteId!, request.ZipFileId!, "Completed", "Zip file successfully created and uploaded.");

                Console.WriteLine($"Zip file {request.ZipFileId} successfully created and uploaded.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
                await TableStorageService.UpdateJobStatus(request.NoteId!, request.ZipFileId!, "Failed", ex.Message);
                throw;
            }
        }
    }

    /// <summary>
    /// Represents a zip request with note and zip file information.
    /// </summary>
    public class ZipRequest
    {
        /// <summary>
        /// Gets or sets the Note ID associated with the attachments.
        /// </summary>
        public string NoteId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the ID of the generated zip file.
        /// </summary>
        public string ZipFileId { get; set; } = string.Empty;
    }

    // BlobStorageService.cs
    /// <summary>
    /// Provides methods to upload zip files to Azure Blob Storage.
    /// </summary>
    public static class BlobStorageService
    {
        /// <summary>
        /// Uploads a zip file to the specified blob container.
        /// </summary>
        /// <param name="containerName">The name of the container to upload the zip file to.</param>
        /// <param name="zipFileId">The name of the zip file blob.</param>
        /// <param name="zipFilePath">The path to the local zip file.</param>
        public static async Task UploadZipToBlob(string containerName, string zipFileId, string zipFilePath)
        {
            // Initialize BlobServiceClient with a connection string
            var blobServiceClient = new BlobServiceClient("DefaultEndpointsProtocol=https;AccountName=sthw4ex2;AccountKey=REPLACE_WITH_YOUR_STORAGE_KEY;EndpointSuffix=core.windows.net");
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
            
            // Ensure the container exists
            await blobContainerClient.CreateIfNotExistsAsync();

            // Get a reference to the blob
            var blobClient = blobContainerClient.GetBlobClient(zipFileId);

            // Upload the zip file
            using (var fileStream = File.OpenRead(zipFilePath))
            {
                await blobClient.UploadAsync(fileStream, true);
            }
        }
    }

    // ZipService.cs
    /// <summary>
    /// Provides methods to create zip archives from note attachments.
    /// </summary>
    public static class ZipService
    {
        /// <summary>
        /// Creates a zip archive from all attachments of the specified note.
        /// </summary>
        /// <param name="noteId">The note ID whose attachments should be zipped.</param>
        /// <returns>The file path of the created zip archive.</returns>
        public static async Task<string> CreateZipFromAttachments(string noteId)
        {
            string zipFilePath = Path.Combine(Path.GetTempPath(), $"{noteId}.zip");

            // Create a zip archive
            using (var zipArchive = System.IO.Compression.ZipFile.Open(zipFilePath, System.IO.Compression.ZipArchiveMode.Create))
            {
                // Get blob service client and container client
                var blobServiceClient = new BlobServiceClient("DefaultEndpointsProtocol=https;AccountName=sthw4ex2;AccountKey=REPLACE_WITH_YOUR_STORAGE_KEY;EndpointSuffix=core.windows.net");
                var containerClient = blobServiceClient.GetBlobContainerClient(noteId);

                // Iterate through all blobs in the container
                await foreach (var blobItem in containerClient.GetBlobsAsync())
                {
                    var blobClient = containerClient.GetBlobClient(blobItem.Name);
                    string tempFilePath = Path.Combine(Path.GetTempPath(), blobItem.Name);

                    // Download blob to temporary file
                    var downloadResponse = await blobClient.DownloadToAsync(tempFilePath);
                    
                    // Add the file to the zip archive
                    zipArchive.CreateEntryFromFile(tempFilePath, blobItem.Name, CompressionLevel.Fastest);
                    
                    // Delete temporary file after adding to the zip
                    File.Delete(tempFilePath);
                }
            }

            return zipFilePath;
        }
    }

    // TableStorageService.cs
    /// <summary>
    /// Provides methods to update the status of jobs in Azure Table Storage.
    /// </summary>
    public static class TableStorageService
    {
        /// <summary>
        /// Updates the status of a zip creation job in Table Storage.
        /// </summary>
        /// <param name="noteId">The note ID associated with the zip request.</param>
        /// <param name="zipFileId">The zip file ID.</param>
        /// <param name="status">The status of the job (e.g., Completed, Failed).</param>
        /// <param name="statusDetails">Additional details about the status.</param>
        public static async Task UpdateJobStatus(string noteId, string zipFileId, string status, string statusDetails)
        {
            // Initialize the TableClient to interact with Table Storage
            var tableClient = new TableClient("DefaultEndpointsProtocol=https;AccountName=sthw4ex2;AccountKey=REPLACE_WITH_YOUR_STORAGE_KEY;EndpointSuffix=core.windows.net", "Jobs");
            await tableClient.CreateIfNotExistsAsync();

            // Create a job status entity
            var jobStatus = new JobStatus
            {
                PartitionKey = noteId,
                RowKey = zipFileId,
                Status = status,
                StatusDetails = statusDetails
            };

            // Upsert the entity to the table
            await tableClient.UpsertEntityAsync(jobStatus);
        }
    }

    /// <summary>
    /// Represents a job status entity stored in Azure Table Storage.
    /// </summary>
    public class JobStatus : ITableEntity
    {
        public string PartitionKey { get; set; } = string.Empty;
        public string RowKey { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusDetails { get; set; } = string.Empty;
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
