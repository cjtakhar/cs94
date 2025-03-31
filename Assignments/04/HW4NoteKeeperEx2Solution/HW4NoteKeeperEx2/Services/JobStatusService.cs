using Azure.Data.Tables;
using NoteKeeper.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace NoteKeeper.Services
{
    public class JobStatusService
    {
        private readonly TableClient _tableClient;
        private readonly ILogger<JobStatusService> _logger;

        public JobStatusService(IConfiguration configuration, ILogger<JobStatusService> logger)
        {
            var storageConnectionString = configuration["AzureWebJobsStorage"];
            _tableClient = new TableClient(storageConnectionString, "Jobs");
            _tableClient.CreateIfNotExists();
            _logger = logger;
        }

        public async Task AddOrUpdateJobStatusAsync(string noteId, string zipFileId, string status, string statusDetails)
        {
            var jobStatus = new JobStatus
            {
                PartitionKey = noteId,
                RowKey = zipFileId,
                Status = status,
                StatusDetails = statusDetails,
                Timestamp = DateTimeOffset.UtcNow
            };

            await _tableClient.UpsertEntityAsync(jobStatus);
            _logger.LogInformation("Updated job status for ZipFileId {ZipFileId} with status {Status}", zipFileId, status);
        }
    }
}
