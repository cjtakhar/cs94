using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using WebJobNetCoreDemo.CustomSettings;

namespace WebJobNetCoreDemo.WebJobFunctions
{
    /// <summary>
    /// Illustrates compressing all messages in a container
    /// </summary>
    /// <remarks>Pass in the name of the container whose images are to be 
    /// compressed to the containertocompress-queue queue
    public class CompressBlobsDemo
    {
        private readonly IStorageSettings _storageSettings;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes the compress with the injected dependencies
        /// </summary>
        /// <param name="storageSettings">The settings needed to access storage</param>
        /// <param name="logger">The logger</param>
        public CompressBlobsDemo(IStorageSettings storageSettings,
                                 ILogger<CompressBlobsDemo> logger)
        {
            _storageSettings = storageSettings;
            _logger = logger;
        }

        /// <summary>
        /// Compresses all images in the container provided to the containertocompress-queue
        /// </summary>
        /// <param name="containerToCompress">The container to compress</param>
        /// <param name="logger">The logger</param>
        /// <returns>A task to await on</returns>
        public async Task ProcessBlobs([QueueTrigger("%sourcequeuename%")] string containerToCompress)
        {
            try
            {
                _logger.LogWarning("Queued Message containerToCompress: [{containerToCompress}]", containerToCompress);

                string targeBlobId = $"{Guid.NewGuid()}.zip";

                // This is the blob that will contain all of the compressed blobs from the containerToCompress
                BlobClient targetClient = GetBlobClient(fileName: targeBlobId, "compressed");

                // This provides access to the container of blobs to compress
                BlobContainerClient sourceClient = GetBlobContainerClient(containerToCompress);

                // Retrieve a list of all the blobs to compress
                Azure.AsyncPageable<BlobItem> blobs = sourceClient.GetBlobsAsync();

                // Create a memory stream to that will contain all of the compressed blobs
                using MemoryStream archiveMemoryStream = new MemoryStream();
                {
                    // Create the ZipArchive that will be used to compress all of the blobs
                    using ZipArchive zipArchive = new ZipArchive(archiveMemoryStream, ZipArchiveMode.Update, leaveOpen: true);
                    {
                        // Loop through all of the blobs and compress them
                        await foreach (var blobPage in blobs.AsPages())
                        {
                            foreach (BlobItem? blobItem in blobPage.Values)
                            {
                                _logger.LogWarning("\tProcessing BlobItem: [{blobItem.Name}]", blobItem.Name);

                                // Get a blob client for the blob to be compressed
                                BlobClient? blobClientToCompress = sourceClient.GetBlobClient(blobItem.Name);
                                using BlobDownloadInfo blobDownloadInfo = await blobClientToCompress.DownloadAsync();
                                {

                                    // Create an entry in the zip archive for the blob
                                    ZipArchiveEntry zipArchiveEntry = zipArchive.CreateEntry(blobItem.Name);

                                    // Open the archive entry's stream so the content of the blob an be written to it
                                    using (Stream writer = zipArchiveEntry.Open())
                                    {
                                        // Copy the blob to the zip archive entry so it will be compressed and
                                        // stored in the zipArchive
                                        await blobDownloadInfo.Content.CopyToAsync(writer);
                                        _logger.LogWarning("\t\tCopied to zipArchiveEntry BlobItem: [{blobItem.Name}]", blobItem.Name);

                                        // Flush the writer so the content is written to the zip archive entry
                                        writer.Flush();
                                    }
                                }
                            }
                        }
                    }

                    // Note:
                    // Dispose of the zip archive so the memory stream can be used to upload the compressed data to the blob
                    // Note: The "using ZipArchive zipArchive = new ZipArchive(archiveMemoryStream, ZipArchiveMode.Update, leaveOpen: true);"
                    // does not cause the data to be written to the memory stream until the zipArchive is disposed by explicitly 
                    // calling zipArchive.Dispose() 
                    zipArchive.Dispose();

                    // Reset the stream to the beginning so all of the content in the stream can be
                    // written to the blob
                    archiveMemoryStream.Position = 0;

                    // Upload the compressed data to the blob in azure storage
                    await targetClient.UploadAsync(archiveMemoryStream, new BlobHttpHeaders() { ContentType = "application/zip" });
                    _logger.LogWarning("\tUploadAsync BlobItem: [{targeBlobId}]", targeBlobId);
                }
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur
                _logger.LogError(ex.Message);
            }
        }

        /// <summary>
        /// Get the blob client for the fileName and containerName provided
        /// </summary>
        /// <param name="fileName">The file name, which is the blob id.</param>
        /// <param name="containerName">The container name</param>
        /// <returns>A blob client representing the fileName and containerName specified</returns>
        private BlobClient GetBlobClient(string fileName, string containerName)
        {
            BlobContainerClient blobContainerClient = GetBlobContainerClient(containerName);

            return blobContainerClient.GetBlobClient(fileName);
        }

        /// <summary>
        /// Get the container client for the storage account
        /// </summary>
        /// <param name="containerName">The container in Azure Storage</param>
        /// <returns>A BlobContainerClient that is connected the the storage account specified in the AzureWebJobsStorage connection string
        /// for the containerName specified</returns>
        private BlobContainerClient GetBlobContainerClient(string containerName)
        {
            BlobContainerClient blobContainerClient = new BlobContainerClient(_storageSettings.ConnectionString, containerName);
            return blobContainerClient;
        }

    }
}
