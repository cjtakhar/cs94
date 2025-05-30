using Azure;
using Azure.Data.Tables;
using System;

namespace NoteKeeper.Models
{
    public class JobStatus : ITableEntity
    {
        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }        
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string? Status { get; set; } 
        public string? StatusDetails { get; set; }
    }
}
