using System.Net;
using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace AzureBlobDemo.Controllers
{
    /// <summary>
    /// Provides access to the public files resource
    /// </summary>
    [Route("api/v1/[controller]")]
    [Produces("application/json")] // See: https://en.wikipedia.org/wiki/Media_type    
    [ApiController]
    public class PublicFilesController : ControllerBase
    {
        /// <summary>
        /// The get public file by identifier route name
        /// </summary>
        private const string GetPublicFileByIdRouteName = nameof(GetPublicFileByIdRouteName);

        /// <summary>
        /// The name of the custom metadata
        /// </summary>
        private const string CustomMetadata = nameof(CustomMetadata);

        /// <summary>
        /// Gets or sets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public IConfiguration Configuration { get; set; }

        /// <summary>
        /// The logger
        /// </summary>
        private ILogger<PublicFilesController> _logger;

        /// <summary>
        /// Gets the storage connection string.
        /// </summary>
        /// <value>
        /// The storage connection string.
        /// </value>
        public string StorageConnectionString
        {
            get
            {
                string? connectionString = Configuration.GetConnectionString("DefaultStorageConnection");
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new Exception("The connection string for the storage account must be provided in the appsettings.json file");
                }
                return connectionString;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PublicFilesController" /> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="logger">The logger.</param>
        public PublicFilesController(IConfiguration configuration,
                                     ILogger<PublicFilesController> logger)
        {
            Configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// The name of the container where public files will be stored in Azure Blob Storage
        /// </summary>
        private const string PUBLIC_FILES_CONTAINER = "publicfilescontainer";

        /// <summary>
        /// Gets a list of all the items
        /// </summary>
        /// <returns>A collection of all the items by name</returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            BlobContainerClient blobContainerClient = new BlobContainerClient(StorageConnectionString, PUBLIC_FILES_CONTAINER);

            List<string> blobNames = [];

            var blobs = blobContainerClient.GetBlobsAsync();

            await foreach (var blobPage in blobs.AsPages())
            {
                foreach (BlobItem blobItem in blobPage.Values)
                {
                    BlobClient blobClient = blobContainerClient.GetBlobClient(blobItem.Name);
                    var blobProperties = blobClient.GetProperties();

                    blobNames.Add(blobItem.Name);
                }
            }

            if (blobNames.Count > 0)
            {
                return new ObjectResult(blobNames.ToArray());
            }

            return new ObjectResult(null);
        }


        /// <summary>
        /// Gets the specified public file by name from the public files container.
        /// </summary>
        /// <param name="id">Name of the public file.</param>
        /// <returns>The contents of the public file</returns>
        [HttpGet(template: "{id}", Name = GetPublicFileByIdRouteName)]

        // These are necessary and typically an API would return one content type however since
        // This api can return anything uploaded the catch all of octet-stream is also included
        // Indicates a file is being returned for 200
        [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(FileResult))]
        // Indicates the types of files being returned
        [Produces("application/pdf", "image/jpeg", "image/png", "text/html", "application/octet-stream")]
        public Task<IActionResult> GetPublicFileById(string id)
        {
            this.Response.Headers.Append("Content-Disposition", "attachment");
            return RetrievePublicFile(containerName: PUBLIC_FILES_CONTAINER, Id: id);
        }

        /// <summary>
        /// Retrieves the public file.
        /// </summary>
        /// <param name="containerName">Name of the container.</param>
        /// <param name="Id">The identifier.</param>
        /// <returns>The file</returns>
        private async Task<IActionResult> RetrievePublicFile(string containerName, string Id)
        {
            try
            {
                // The container name must be lower case
                containerName = containerName.ToLower();

                // Validate the container name and Id
                if (string.IsNullOrWhiteSpace(containerName))
                {
                    return BadRequest("The container name must be provided");
                }

                if (string.IsNullOrWhiteSpace(Id))
                {
                    return BadRequest("The Id must be provided");
                }

                // Create a blob client
                BlobContainerClient blobContainerClient = new BlobContainerClient(StorageConnectionString, PUBLIC_FILES_CONTAINER);

                // Get the blob client
                BlobClient blobClient = blobContainerClient.GetBlobClient(Id);

                // Download the blob's contents 
                using BlobDownloadInfo blobDownloadInfo = await blobClient.DownloadAsync();

                // Create a memory stream to hold the blob's contents
                MemoryStream memoryStream = new MemoryStream();

                // Copy the blob's contents to the memory stream
                await blobDownloadInfo.Content.CopyToAsync(memoryStream);

                // Reset the stream to the beginning so readers don't have to
                memoryStream.Position = 0;

                // Indicate that the file is an attachment and should be downloaded
                var contentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                {
                    // Set the file name
                    FileName = Id
                };

                // Add the content disposition header to the response
                // This is necessary for the browser to know that a file is being returned
                Response.Headers.Append("Content-Disposition", contentDisposition.ToString());

                // Return the file with the specific contentType and name, which is in the id parameter
                return File(memoryStream, blobDownloadInfo.ContentType, Id);

            }
            catch (Azure.RequestFailedException ex)
            {
                _logger.LogWarning(ex.Message);
                return StatusCode(ex.Status);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Uploads a file
        /// </summary>
        /// <param name="file">The file uploaded.</param>
        /// <param name="publicFileName">Name of the public file.</param>
        /// <returns>
        /// The name of the blob entry created
        /// </returns>
        /// <remarks>The publicFileName is the Id of the blob</remarks>
        [ProducesDefaultResponseType]
        [HttpPut]
        public async Task<IActionResult> PutPublicFileImage(IFormFile file, string publicFileName)
        {
            try
            {
                // Create a container client
                BlobContainerClient blobContainerClient = new BlobContainerClient(StorageConnectionString, PUBLIC_FILES_CONTAINER);

                // Get the blob client with reference to the blob name
                BlobClient blobClient = blobContainerClient.GetBlobClient(publicFileName);

                // Create the container if it doesn't already exist.
                await blobContainerClient.CreateIfNotExistsAsync();

                // Set permissions on the blob container to allow public access
                await blobContainerClient.SetAccessPolicyAsync(PublicAccessType.Blob);

                // Determines if the blob already exists
                bool blobUpdated = await blobClient.ExistsAsync();

                // Get the current date and time in universal time
                string dateTime = DateTime.UtcNow.ToString("o");

                // Upload the file
                using Stream fileStream = file.OpenReadStream();
                await blobClient.UploadAsync(fileStream, new BlobHttpHeaders() { ContentType = file.ContentType });

                // Set the metadata for the blob
                // Must be done after the blob is uploaded

                // Set the metadata for the blob
                if (blobUpdated)
                {
                    await blobClient.SetMetadataAsync(new Dictionary<string, string>
                    {
                        { CustomMetadata, $"My custom metadata updated {dateTime}" }
                    });
                }
                else
                {
                    await blobClient.SetMetadataAsync(new Dictionary<string, string>
                    {
                        { CustomMetadata, $"My custom metadata created {dateTime}" }
                    });
                }

                // Return the appropriate response, either 204 No Content or 201 Created
                // The 204 No Content response indicates that the request was successful
                // and there is no content to return
                if (blobUpdated)
                {
                    return NoContent();
                }

                // The 201 Created response indicates that the request was successful
                // and a new resource was created
                // The URI of the newly created resource is returned in the Location header
                return CreatedAtRoute(GetPublicFileByIdRouteName, new { id = publicFileName }, null);
            }
            catch (Azure.RequestFailedException ex)
            {
                _logger.LogWarning(ex.Message);
                return StatusCode(ex.Status);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
            finally
            {
                // Perform any cleanup or resource release here
            }
        }

        /// <summary>
        /// Uploads a file
        /// </summary>
        /// <returns>
        /// The name of the blob entry created
        /// </returns>
        /// <remarks>The server assigns the ID of the blob</remarks>
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Created)]
        [HttpPost]
        public async Task<IActionResult> PostPublicFileImage(IFormFile file)
        {
            // Generate a new GUID for the blob name
            string fileName = Guid.NewGuid().ToString().ToLower();

            // Create a container client
            BlobContainerClient blobContainerClient = new BlobContainerClient(StorageConnectionString, PUBLIC_FILES_CONTAINER);

            // Create the container if it doesn't already exist.
            await blobContainerClient.CreateIfNotExistsAsync();

            // Get the blob client with reference to the blob name
            BlobClient blobClient = blobContainerClient.GetBlobClient(fileName);

            // Set permissions on the blob container to allow public access
            await blobContainerClient.SetAccessPolicyAsync(PublicAccessType.Blob);

            // Get the current date and time in universal time
            string dateTime = DateTime.UtcNow.ToString("o");

            // Upload the file
            using Stream fileStream = file.OpenReadStream();
            await blobClient.UploadAsync(fileStream, new BlobHttpHeaders() { ContentType = file.ContentType });

            // Set the metadata for the blob
            // Must be done after the blob is uploaded
            await blobClient.SetMetadataAsync(new Dictionary<string, string>
                    {
                        { CustomMetadata, $"My custom metadata created {dateTime}" }
                    });


            // The 201 Created response indicates that the request was successful
            // and a new resource was created
            // The URI of the newly created resource is returned in the Location header
            return CreatedAtRoute(GetPublicFileByIdRouteName, new { id = fileName }, null);
        }
    }

}
