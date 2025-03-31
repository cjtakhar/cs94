using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.ApplicationInsights;
using Azure.Storage.Queues;
using Azure.Storage.Blobs;
using Azure.Data.Tables;
using NoteKeeper.Data;
using System.Text.Json;
using NoteKeeper.Services;
using NoteKeeper.Models;
using System.Collections.Generic;

namespace NoteKeeper.Controllers
{
    /// <summary>
    /// Controller responsible for managing zip archive operations and tracking job status for note attachments.
    /// </summary>
    [ApiController]
    [Route("notes/{noteId}/attachmentzipfiles")]
    public class AttachmentZipFilesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly TelemetryClient _telemetryClient;
        private readonly QueueClient _queueClient;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly TableClient _tableClient;
        private readonly ILogger<AttachmentZipFilesController> _logger;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes the controller and sets up Azure Storage clients.
        /// </summary>
        public AttachmentZipFilesController(
            AppDbContext context,
            TelemetryClient telemetryClient,
            ILogger<AttachmentZipFilesController> logger,
            IConfiguration configuration)
        {
            _context = context;
            _telemetryClient = telemetryClient;
            _logger = logger;
            _configuration = configuration;

            // Initialize queue and table clients for zip requests and job tracking
            string queueConnectionString = _configuration["Storage:ConnectionString"]
                ?? throw new InvalidOperationException("Storage connection string is not set.");

            _queueClient = new QueueClient(queueConnectionString, "attachment-zip-requests-ex1");
            _queueClient.CreateIfNotExists();

            _blobServiceClient = new BlobServiceClient(queueConnectionString);
            _tableClient = new TableClient(queueConnectionString, "Jobs");
            _tableClient.CreateIfNotExists();
        }

        /// <summary>
        /// Enqueues a request to create a zip file of all attachments for a note and tracks the job status.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RequestZipFile(string noteId)
        {
            // Validate noteId format
            if (!Guid.TryParse(noteId, out var parsedNoteId))
                return BadRequest("Invalid note ID format.");

            // Check if note exists in the database
            var noteExists = await _context.Notes.AnyAsync(n => n.NoteId == parsedNoteId);
            if (!noteExists)
            {
                _logger.LogWarning("Note {NoteId} not found for zip request.", noteId);
                return NotFound();
            }

            // Check if attachments exist for the note
            var containerClient = _blobServiceClient.GetBlobContainerClient(noteId);
            if (!await containerClient.ExistsAsync())
                return NoContent();

            int attachmentCount = 0;
            await foreach (var _ in containerClient.GetBlobsAsync())
            {
                attachmentCount++;
                break;
            }

            if (attachmentCount == 0)
                return NoContent();

            // Generate a unique zip file ID and enqueue a message to the queue
            string zipFileId = $"{Guid.NewGuid()}.zip";
            var messagePayload = new { noteId, zipFileId };
            string messageJson = JsonSerializer.Serialize(messagePayload);
            await _queueClient.SendMessageAsync(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(messageJson)));

            // Add initial job status to Azure Table Storage
            var jobStatus = new JobStatus
            {
                PartitionKey = noteId,
                RowKey = zipFileId,
                Status = "Queued",
                StatusDetails = $"Queued: Zip File Id: {zipFileId} NoteId: {noteId}"
            };

            await _tableClient.UpsertEntityAsync(jobStatus);
            _logger.LogInformation("Job queued successfully. ZipFileId: {ZipFileId}, NoteId: {NoteId}", zipFileId, noteId);

            // Set Location header to where the zip file will be available
            string blobUrl = $"{_configuration["BlobBaseUrl"]}/{noteId}-zip/{zipFileId}";
            Response.Headers.Location = blobUrl;
            return Accepted();
        }

        /// <summary>
        /// Retrieves a specific zip file by zipFileId for a given note.
        /// </summary>
        [HttpGet("{zipFileId}")]
        public async Task<IActionResult> GetZipFile(string noteId, string zipFileId)
        {
            if (!Guid.TryParse(noteId, out var parsedNoteId))
                return BadRequest("Invalid note ID format.");

            var noteExists = await _context.Notes.AnyAsync(n => n.NoteId == parsedNoteId);
            if (!noteExists)
            {
                _logger.LogWarning("Note {NoteId} not found when attempting to retrieve zip file.", noteId);
                return NotFound();
            }

            var containerClient = _blobServiceClient.GetBlobContainerClient($"{noteId}-zip");
            var blobClient = containerClient.GetBlobClient(zipFileId);

            if (!await blobClient.ExistsAsync())
            {
                _logger.LogWarning("Zip file {ZipFileId} not found for note {NoteId}.", zipFileId, noteId);
                return NotFound();
            }

            var downloadResponse = await blobClient.DownloadStreamingAsync();

            return File(
                downloadResponse.Value.Content,
                "application/zip",
                fileDownloadName: zipFileId
            );
        }

        /// <summary>
        /// Lists all zip files created for the given note.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllZipFiles(string noteId)
        {
            if (!Guid.TryParse(noteId, out var parsedNoteId))
                return BadRequest("Invalid note ID format.");

            var noteExists = await _context.Notes.AnyAsync(n => n.NoteId == parsedNoteId);
            if (!noteExists)
            {
                _logger.LogWarning("Note {NoteId} not found when retrieving zip archive list.", noteId);
                return NotFound();
            }

            var containerClient = _blobServiceClient.GetBlobContainerClient($"{noteId}-zip");

            var results = new List<object>();

            await foreach (var blobItem in containerClient.GetBlobsAsync())
            {
                results.Add(new
                {
                    zipFileId = blobItem.Name,
                    contentType = blobItem.Properties.ContentType ?? "application/zip",
                    createdDate = blobItem.Properties.CreatedOn ?? DateTimeOffset.MinValue,
                    lastModifiedDate = blobItem.Properties.LastModified ?? DateTimeOffset.MinValue,
                    length = blobItem.Properties.ContentLength ?? 0
                });
            }

            return Ok(results);
        }

        /// <summary>
        /// Retrieves job status for a specific zip file request.
        /// </summary>
        [HttpGet("jobs/{zipFileId}")]
        public async Task<IActionResult> GetJobStatus(string noteId, string zipFileId)
        {
            var jobEntity = await _tableClient.GetEntityIfExistsAsync<JobStatus>(noteId, zipFileId);

            if (!jobEntity.HasValue || jobEntity.Value == null)  // <-- Null check added
            {
                _logger.LogWarning("Job status for ZipFileId {ZipFileId} not found.", zipFileId);
                return NotFound();
            }

            var result = jobEntity.Value!;  // Safe to use after null check
            return Ok(new
            {
                ZipFileId = result.RowKey,
                TimeStamp = result.Timestamp,
                Status = result.Status,
                StatusDetails = result.StatusDetails
            });
        }


        /// <summary>
        /// Retrieves all job statuses for a given note.
        /// </summary>
        [HttpGet("jobs")]
        public async Task<IActionResult> GetAllJobStatuses(string noteId)
        {
            var queryResults = _tableClient.QueryAsync<JobStatus>(x => x.PartitionKey == noteId);

            var results = new List<object>();
            await foreach (var job in queryResults)
            {
                results.Add(new
                {
                    ZipFileId = job.RowKey,
                    TimeStamp = job.Timestamp,
                    Status = job.Status,
                    StatusDetails = job.StatusDetails
                });
            }

            return Ok(results);
        }

        /// <summary>
        /// Deletes a zip file and its associated job status.
        /// </summary>
        [HttpDelete("{zipFileId}")]
        public async Task<IActionResult> DeleteZipFile(string noteId, string zipFileId)
        {
            if (!Guid.TryParse(noteId, out var parsedNoteId))
                return BadRequest("Invalid note ID format.");

            var containerClient = _blobServiceClient.GetBlobContainerClient($"{noteId}-zip");
            var blobClient = containerClient.GetBlobClient(zipFileId);

            if (!await blobClient.ExistsAsync())
            {
                _logger.LogWarning("Zip file {ZipFileId} not found for note {NoteId}.", zipFileId, noteId);
                return NoContent();
            }

            await blobClient.DeleteIfExistsAsync();

            // Delete job status
            await _tableClient.DeleteEntityAsync(noteId, zipFileId);
            _logger.LogInformation("Zip file and job status deleted successfully. ZipFileId: {ZipFileId}", zipFileId);

            return NoContent();
        }
    }
}
