using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.IO;
using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Azure.Storage.Blobs;


namespace BlobTriggerDemo
{
    public class ProcessUploadedImages
    {
        private readonly ILogger _logger;

        public ProcessUploadedImages(ILogger<ProcessUploadedImages> logger)
        {
            _logger = logger;
        }

        const string ImagesToConvertRoute = "imagestoconverttograyscale/{name}";

        /// <summary>
        /// Converts images uploaded into the imagestoconverttograyscale into gray scale format
        /// If success the convertedimages container contains the result image
        /// If fail the failedimages container contains the original image uploaded into the
        /// imagestoconverttograyscale continer
        /// 
        /// An initial job record is added to the jobs table indicating the status of the job
        /// </summary>
        /// <param name="blobStream">The BLOB stream.</param>
        /// <param name="name">The name.</param>
        /// <param name="log">The log.</param>
        /// <remarks>
        /// You will need to create a local.settings.json file to run this from visual studio
        /// See ReadMe.txt
        /// </remarks>
        [Function("ProcessUploadedImages")]
        public async Task Run([BlobTrigger(ImagesToConvertRoute, Connection = ConfigSettings.STORAGE_CONNECTIONSTRING_NAME)] BlobClient blobClientWithBlobToBeProcessed, string name)
        {
            Azure.Storage.Blobs.Models.BlobProperties properties = (await blobClientWithBlobToBeProcessed.GetPropertiesAsync()).Value;
            _logger.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n ContentType: {properties.ContentType} Bytes");

            using (Stream blobStream = await blobClientWithBlobToBeProcessed.OpenReadAsync())
            {
                // Get the storage account
                string storageConnectionString = Environment.GetEnvironmentVariable(ConfigSettings.STORAGE_CONNECTIONSTRING_NAME);
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);

                // Create a blob client so blobs can be retrieved and created
                CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

                // Create or retrieve a reference to the converted images container
                CloudBlobContainer convertedImagesContainer = cloudBlobClient.GetContainerReference(ConfigSettings.CONVERTED_IMAGES_CONTAINERNAME);
                bool created = await convertedImagesContainer.CreateIfNotExistsAsync();
                _logger.LogInformation($"[{ConfigSettings.CONVERTED_IMAGES_CONTAINERNAME}] Container needed to be created: {created}");

                CloudBlobContainer failedImagesContainer = cloudBlobClient.GetContainerReference(ConfigSettings.FAILED_IMAGES_CONTAINERNAME);
                created = await failedImagesContainer.CreateIfNotExistsAsync();
                _logger.LogInformation($"[{ConfigSettings.FAILED_IMAGES_CONTAINERNAME}] Container needed to be created: {created}");

                await ConvertAndStoreImage(_logger, blobStream, convertedImagesContainer, name, failedImagesContainer);
            }
        }

        /// <summary>
        /// Converts the and store image.
        /// </summary>
        /// <param name="log">The log.</param>
        /// <param name="uploadedImagesContainer">The uploaded images container.</param>
        /// <param name="convertedImagesContainer">The converted images container.</param>
        /// <param name="blobName">Name of the BLOB.</param>
        private static async Task ConvertAndStoreImage(ILogger log,
                                                 Stream uploadedImage,
                                                 CloudBlobContainer convertedImagesContainer,
                                                 string blobName,
                                                 CloudBlobContainer failedImagesContainer)
        {
            string convertedBlobName = $"{Guid.NewGuid()}-{blobName}";
            string jobId = Guid.NewGuid().ToString();

            try
            {
                await UpdateJobTableWithStatus(log, jobId, status: "Processing image...", message: "Received the blob ready to process it.");

                uploadedImage.Seek(0, SeekOrigin.Begin);

                using (MemoryStream convertedMemoryStream = new MemoryStream())
                using (var image = Image.Load(uploadedImage))
                {
                    log.LogInformation($"[+] Starting conversion of image {blobName}");

                    image.Mutate(x => x.Grayscale());
                    image.SaveAsJpeg(convertedMemoryStream);

                    convertedMemoryStream.Seek(0, SeekOrigin.Begin);
                    log.LogInformation($"[-] Completed conversion of image {blobName}");

                    log.LogInformation($"[+] Storing converted image {blobName} into {ConfigSettings.CONVERTED_IMAGES_CONTAINERNAME} container");

                    CloudBlockBlob convertedBlockBlob = convertedImagesContainer.GetBlockBlobReference(convertedBlobName);

                    convertedBlockBlob.Metadata.Add(ConfigSettings.JOBID_METADATA_NAME, jobId);

                    convertedBlockBlob.Properties.ContentType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                    await convertedBlockBlob.UploadFromStreamAsync(convertedMemoryStream);

                    // Save the job id in blob meta data for later retrieval
                    //convertedBlockBlob.SetMetadata(null);

                    log.LogInformation($"[-] Stored converted image {convertedBlobName} into {ConfigSettings.CONVERTED_IMAGES_CONTAINERNAME} container");

                }
            }
            catch (Exception ex)
            {
                log.LogError($"Failed to convert blob {blobName} Exception ex {ex.Message}");
                await StoreFailedImage(log, uploadedImage, blobName, failedImagesContainer, convertedBlobName: convertedBlobName, jobId: jobId);
            }
        }
        /// <summary>
        /// Updates the job table with status.
        /// </summary>
        /// <param name="log">The log.</param>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="status">The status.</param>
        /// <param name="message">The message.</param>
        private static async Task UpdateJobTableWithStatus(ILogger log, string jobId, string status, string message)
        {
            JobTable jobTable = new JobTable(log, ConfigSettings.IMAGEJOBS_PARTITIONKEY);
            await jobTable.InsertOrReplaceJobEntity(jobId, status: status, message: message);
        }

        /// <summary>
        /// Stores the failed image.
        /// </summary>
        /// <param name="log">The log.</param>
        /// <param name="uploadedImage">The uploaded image.</param>
        /// <param name="blobName">Name of the BLOB.</param>
        /// <param name="failedImagesContainer">The failed images container.</param>
        /// <param name="convertedBlobName">Name of the converted BLOB.</param>
        /// <param name="jobId">The job identifier.</param>
        private static async Task StoreFailedImage(ILogger log, Stream uploadedImage, string blobName, CloudBlobContainer failedImagesContainer, string convertedBlobName, string jobId)
        {
            try
            {
                log.LogInformation($"[+] Storing failed image {blobName} into {ConfigSettings.FAILED_IMAGES_CONTAINERNAME} container as blob name: {convertedBlobName}");

                CloudBlockBlob failedBlockBlob = failedImagesContainer.GetBlockBlobReference(convertedBlobName);
                failedBlockBlob.Metadata.Add(ConfigSettings.JOBID_METADATA_NAME, jobId);

                uploadedImage.Seek(0, SeekOrigin.Begin);
                await failedBlockBlob.UploadFromStreamAsync(uploadedImage);

                log.LogInformation($"[+] Stored failed image {blobName} into {ConfigSettings.FAILED_IMAGES_CONTAINERNAME} container as blob name: {convertedBlobName}");
            }
            catch (Exception ex)
            {
                log.LogError($"Failed to store a blob called {blobName} that failed conversion into {ConfigSettings.FAILED_IMAGES_CONTAINERNAME}. Exception ex {ex.Message}");
            }
        }
    }
}
