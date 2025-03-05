using System.Net;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using AzureBlobDemo.DataTransferObjects;
using Microsoft.AspNetCore.Mvc;
using System.Text;


namespace AzureBlobDemo.Controllers
{
    /// <summary>
    /// Provides access to the secret message resource
    /// </summary>
    [Route("api/v1/[controller]")]
    [Produces("application/json")] // See: https://en.wikipedia.org/wiki/Media_type    
    [ApiController]
    public class SecretMessagesController : ControllerBase
    {

        /// <summary>
        /// The get secret message by identifier route name
        /// </summary>
        private const string GetSecretMessageByIdRouteName = nameof(GetSecretMessageByIdRouteName);

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
        private ILogger<SecretMessagesController> _logger;

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
        /// Initializes a new instance of the <see cref="PublicMessagesController" /> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="logger">The logger.</param>
        public SecretMessagesController(IConfiguration configuration,
                                        ILogger<SecretMessagesController> logger)
        {
            Configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// The name of the container where secret items will be stored in Azure Blob Storage
        /// </summary>
        private const string SECRET_MESSAGES_CONTAINER = "secretmessagescontainer";

        /// <summary>
        /// Gets a list of all the secret messages
        /// </summary>
        /// <returns>A collection of all the secret messages by name</returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {

            BlobContainerClient blobContainerClient = new BlobContainerClient(StorageConnectionString, SECRET_MESSAGES_CONTAINER);

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
        /// Gets the specified secret message by name from the secret container.
        /// </summary>
        /// <param name="id">Name of the secret message.</param>
        /// <returns>The contents of the secret message</returns>
        [HttpGet("{id}", Name = GetSecretMessageByIdRouteName)]
        public Task<IActionResult> GetById(string id)
        {
            return RetrieveSecretMessage(containerName: SECRET_MESSAGES_CONTAINER, Id: id);
        }

        /// <summary>
        /// Creates a message in blob storage
        /// </summary>
        /// <param name="messageName">Name of the message.</param>
        /// <param name="message">The message.</param>
        /// <returns>The location and name of the blob created</returns>     
        /// <remarks>Messages created are publicly accessible</remarks>
        /// <remarks>The Id of the message is the the messageName</remarks>
        [HttpPut("{messageName}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> PutByNameAndMessage(string messageName, [FromBody] string message)
        {
            try
            {
                // Create a container client
                BlobContainerClient blobContainerClient = new BlobContainerClient(StorageConnectionString, SECRET_MESSAGES_CONTAINER);

                // Get the blob client with reference to the blob name
                BlobClient blobClient = blobContainerClient.GetBlobClient(messageName);

                // Create the container if it doesn't already exist.
                await blobContainerClient.CreateIfNotExistsAsync();

                // Set permissions on the blob container to allow public access
                await blobContainerClient.SetAccessPolicyAsync(PublicAccessType.Blob);

                // Determines if the blob already exists
                bool blobUpdated = await blobClient.ExistsAsync();

                // Create a MemoryStream that contains the serializedInput
                using MemoryStream messageAsStream = new MemoryStream(Encoding.UTF8.GetBytes(message));

                // Create or overwrite the blob with contents of the message provided
                await blobClient.UploadAsync(messageAsStream, new BlobHttpHeaders() { ContentType = System.Net.Mime.MediaTypeNames.Text.Plain });

                if (blobUpdated)
                {
                    return NoContent();
                }

                CreateResponsePayload response = new CreateResponsePayload()
                {
                    id = messageName,
                    message = message
                };

                // The 201 Created response indicates that the request was successful
                // and a new resource was created
                // The URI of the newly created resource is returned in the Location header
                return CreatedAtRoute(GetSecretMessageByIdRouteName,
                    new { id = response.id }, response);
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
        /// Creates a secret message
        /// </summary>
        /// <param name="inputPayload">The input payload.</param>
        /// <returns>The location and name of the blob entry created, the name is a GUID and is assigned by the the server</returns>        
        /// <remarks>The server assigns the ID of the message</remarks>
        [HttpPost]
        [ProducesResponseType(typeof(CreateResponsePayload), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PostByMessage([FromBody] InputPayload inputPayload)
        {
            if (inputPayload == null || string.IsNullOrWhiteSpace(inputPayload?.message))
            {
                return BadRequest("message must be JSON in the form { \"message\":\"string\"} and the message content must be non-null");
            }

            try
            {
                // Create a container client
                BlobContainerClient blobContainerClient = new BlobContainerClient(StorageConnectionString, SECRET_MESSAGES_CONTAINER);

                // Get the blob client with reference to the blob name
                // Because this is a POST the blob name is generated by the server
                BlobClient blobClient = blobContainerClient.GetBlobClient(Guid.NewGuid().ToString());

                // Create the container if it doesn't already exist.
                await blobContainerClient.CreateIfNotExistsAsync();

                // Set permissions on the blob container to allow public access
                await blobContainerClient.SetAccessPolicyAsync(PublicAccessType.None);

                // Create a MemoryStream that contains the serializedInput
                using MemoryStream messageAsStream = new MemoryStream(Encoding.UTF8.GetBytes(inputPayload.message));

                // Create or overwrite the blob with contents of the message provided
                await blobClient.UploadAsync(messageAsStream, new BlobHttpHeaders() { ContentType = System.Net.Mime.MediaTypeNames.Text.Plain });

                CreateResponsePayload response = new CreateResponsePayload()
                {
                    id = blobClient.Name,
                    message = inputPayload.message
                };

                // The 201 Created response indicates that the request was successful
                // and a new resource was created
                // The URI of the newly created resource is returned in the Location header
                return CreatedAtRoute(GetSecretMessageByIdRouteName,
                    new { id = response.id }, response);
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
        /// Retrieves the secret message.
        /// </summary>
        /// <param name="Id">The secret message identifier.</param>
        /// <param name="containerName">Name of the container.</param>
        /// <returns>The secret message content</returns>
        /// <remarks>
        /// A container name must be a valid DNS name, conforming to the following naming rules:
        ///  1.Container names must start with a letter or number, and can contain only letters, numbers, and the dash (-) character.
        ///  2.Every dash (-) character must be immediately preceded and followed by a letter or number; consecutive dashes are not permitted in container names.
        ///  3.All letters in a container name must be lowercase.
        ///  4.Container names must be from 3 through 63 characters long.
        /// 
        /// A blob name must conforming to the following naming rules:
        ///  A blob name can contain any combination of characters.
        ///  A blob name must be at least one character long and cannot be more than 1,024 characters long.
        ///  Blob names are case-sensitive.
        ///  Reserved URL characters must be properly escaped.
        ///  The number of path segments comprising the blob name cannot exceed 254. A path segment is the string between consecutive delimiter characters(e.g., the forward slash '/') that corresponds to the name of a virtual directory.        
        /// </remarks>
        private async Task<IActionResult> RetrieveSecretMessage(string containerName, string Id)
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
                BlobContainerClient blobContainerClient = new BlobContainerClient(StorageConnectionString, SECRET_MESSAGES_CONTAINER);

                // Get the blob client
                BlobClient blobClient = blobContainerClient.GetBlobClient(Id);

                // Verify we have the container requested.
                if (!await blobContainerClient.ExistsAsync())
                {
                    return BadRequest($"The Container {containerName} does not exist");
                }

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
                    FileName = $"{Id}.txt"
                };

                // Add the content disposition header to the response
                // This is necessary for the browser to know that a file is being returned
                Response.Headers.Append("Content-Disposition", contentDisposition.ToString());

                // Return the file with the specific contentType and name, which is in the id parameter
                return File(memoryStream, blobDownloadInfo.ContentType, contentDisposition.FileName);

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

    }

}
