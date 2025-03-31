using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace NoteKeeper.Controllers
{
    /// <summary>
    /// Controller for managing attachments for notes.
    /// </summary>
    [ApiController]
    [Route("notes/{noteId}/attachments")]
    public class AttachmentsController : ControllerBase
    {
        private readonly BlobServiceClient _blobServiceClient; // BlobServiceClient is a client for Azure Blob Storage
        private readonly ILogger<AttachmentsController> _logger; // ILogger is a generic interface for logging
        private readonly TelemetryClient _telemetryClient; // TelemetryClient is a client for Azure Application Insights
        private readonly int _maxAttachments; // Maximum number of attachments allowed per note

        public AttachmentsController(IConfiguration configuration, ILogger<AttachmentsController> logger, TelemetryClient telemetryClient)
        {
            _logger = logger;
            string connectionString = configuration.GetSection("Storage")["ConnectionString"] // Get the Azure Storage connection string from configuration         
                                      ?? throw new InvalidOperationException("Azure Storage connection string is not configured.");
            _maxAttachments = int.Parse(configuration["Storage:MaxAttachments"] ?? "3"); // Get the maximum number of attachments allowed per note from configuration
            _blobServiceClient = new BlobServiceClient(connectionString); // Create a new BlobServiceClient using the connection string
            _telemetryClient = telemetryClient; // Set the TelemetryClient
        }

        private void LogException(Exception ex, object context)
        {
            _logger.LogError(ex, $"Exception: {ex.Message}, Context: {context}");
        }

        private void LogValidationError(string errorDetails, object context)
        {
            _logger.LogWarning($"Validation error: {errorDetails}, Context: {context}");
        }

        /// <summary>
        /// Upload an attachment to a note.
        /// </summary>
        /// <param name="noteId"></param>
        /// <param name="attachmentId"></param>
        /// <param name="fileData"></param>
        /// <returns></returns>
        [HttpPut("{attachmentId}")]
        public async Task<IActionResult> UploadAttachment(string noteId, string attachmentId, IFormFile fileData)
        {
            // Validate file data
            if (fileData == null || fileData.Length == 0)
            {
                var errorDetails = "File is required.";
                LogValidationError(errorDetails, new { NoteId = noteId, AttachmentId = attachmentId });
                return BadRequest(errorDetails);
            }

            try
            {
                // Check if note exists
                var containerClient = _blobServiceClient.GetBlobContainerClient(noteId);
                bool containerExists = await containerClient.ExistsAsync();

                if (!containerExists)
                {
                    await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);
                }
                // Check if attachment limit is reached
                int count = 0;
                await foreach (var page in containerClient.GetBlobsAsync().AsPages(pageSizeHint: _maxAttachments + 1))
                {
                    count += page.Values.Count;
                    if (count >= _maxAttachments) break;
                }

                if (count >= _maxAttachments)
                {
                    return Problem(
                        detail: $"Attachment limit reached. MaxAttachments [{_maxAttachments}]",
                        statusCode: 403,
                        title: "Attachment limit reached"
                    );
                }

                // Upload the attachment
                var blobClient = containerClient.GetBlobClient(attachmentId);
                bool blobExists = await blobClient.ExistsAsync();

                using (var stream = new MemoryStream())
                {
                    await fileData.CopyToAsync(stream);
                    stream.Position = 0;
                    await blobClient.UploadAsync(stream, new BlobUploadOptions
                    {
                        HttpHeaders = new BlobHttpHeaders { ContentType = fileData.ContentType },
                        Metadata = new Dictionary<string, string> { { "NoteId", noteId } }
                    });
                }

                // Log telemetry
                string eventName = blobExists ? "AttachmentUpdated" : "AttachmentCreated";
                var telemetry = new EventTelemetry(eventName);
                telemetry.Properties["AttachmentId"] = blobClient.Name;
                telemetry.Metrics["AttachmentSize"] = fileData.Length;
                _telemetryClient.TrackEvent(telemetry);

                return blobExists ? NoContent() : Created(blobClient.Uri.ToString(), new { AttachmentUrl = blobClient.Uri.ToString() });
            }
            catch (Exception ex)
            {
                LogException(ex, new { NoteId = noteId, AttachmentId = attachmentId });
                return StatusCode(500, "Internal Server Error");
            }
        }

        /// <summary>
        /// Delete an attachment from a note.
        /// </summary>
        /// <param name="noteId"></param>
        /// <param name="attachmentId"></param>
        /// <returns></returns>

        [HttpDelete("{attachmentId}")]
        public async Task<IActionResult> DeleteAttachment(string noteId, string attachmentId)
        {
            // Check if note exists
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(noteId);
                if (!await containerClient.ExistsAsync())
                {
                    _logger.LogWarning($"Note {noteId} not found. Cannot delete attachment {attachmentId}.");
                    return NotFound($"Note {noteId} does not exist.");
                }

                // Check if attachment exists
                var blobClient = containerClient.GetBlobClient(attachmentId);
                bool deleted = await blobClient.DeleteIfExistsAsync();

                if (deleted)
                {
                    _logger.LogInformation($"Attachment {attachmentId} deleted successfully from note {noteId}.");
                    return NoContent();
                }
                else
                {
                    return NotFound($"Attachment {attachmentId} does not exist.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting attachment {attachmentId} from note {noteId}: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }

        /// <summary>
        /// Get an attachment from a note.
        /// </summary>
        /// <param name="noteId"></param>
        /// <param name="attachmentId"></param>
        /// <returns></returns>

        [HttpGet("{attachmentId}")]
        public async Task<IActionResult> GetAttachment(string noteId, string attachmentId)
        {
            // Check if note exists
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(noteId);
                if (!await containerClient.ExistsAsync())
                {
                    return NotFound($"Note {noteId} does not exist.");
                }

                // Check if attachment exists
                var blobClient = containerClient.GetBlobClient(attachmentId);
                if (!await blobClient.ExistsAsync())
                {
                    return NotFound($"Attachment {attachmentId} does not exist.");
                }

                var downloadResponse = await blobClient.DownloadStreamingAsync();
                return File(downloadResponse.Value.Content, downloadResponse.Value.Details.ContentType, attachmentId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving attachment {attachmentId} from note {noteId}: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }

        /// <summary>
        /// Get all attachments for a note. 
        /// </summary>
        /// <param name="noteId"></param>
        /// <returns></returns>

        [HttpGet]
        public async Task<IActionResult> GetAllAttachments(string noteId)
        {
            // Check if note exists
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(noteId);
                if (!await containerClient.ExistsAsync())
                {
                    return NotFound($"Note {noteId} does not exist.");
                }

                // Get all attachments
                var attachments = new List<object>();
                await foreach (var blobItem in containerClient.GetBlobsAsync())
                {
                    attachments.Add(new { attachmentId = blobItem.Name, contentType = blobItem.Properties.ContentType, size = blobItem.Properties.ContentLength });
                }

                return Ok(new { noteId, attachments });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving attachments for note {noteId}: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}
