using Azure;
using Azure.Data.Tables;
using AzureTables2;

namespace AzureTablesDemo2.Models
{
    /// <summary>
    /// Defines the input payload for an internal log entry
    /// </summary>
    public class InternalLogEntryInput
    {
        /// <summary>
        /// Gets or sets the causality identifier.
        /// </summary>
        /// <value>The causality identifier.</value>
        public Guid CausalityId { get; set; }
        /// <summary>
        /// Gets or sets the summary of the issue.
        /// </summary>
        /// <value>The summary.</value>
        public string? Summary { get; set; }
        /// <summary>
        /// Gets or sets the issue context, details associated with the issue for debugging purposes.
        /// </summary>
        /// <value>The issue context.</value>
        public string? IssueContext { get; set; }
    }

    /// <summary>
    /// Describes the full result payload for an input log entry
    /// </summary>
    public class InternalLogEntry : ITableEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InternalLogEntry"/> class.
        /// </summary>
        public InternalLogEntry()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InternalLogEntry"/> class.
        /// </summary>
        /// <param name="logEntryInput">The log entry input to initialize this instance with.</param>
        public InternalLogEntry(InternalLogEntryInput logEntryInput)
        {
            CausalityId = logEntryInput.CausalityId;
            Summary = logEntryInput.Summary;
            IssueContext = logEntryInput.IssueContext;
            PartitionKey = TableConstants.INTERNALLOG_PARTITIONKEY;
        }

        /// <summary>
        /// Gets or sets the causality identifier.
        /// </summary>
        /// <value>The causality identifier.</value>
        public Guid CausalityId { get; set; }
        /// <summary>
        /// Gets or sets the summary of the issue.
        /// </summary>
        /// <value>The summary.</value>
        public string? Summary { get; set; }
        /// <summary>
        /// Gets or sets the issue context, details associated with the issue for debugging purposes.
        /// </summary>
        /// <value>The issue context.</value>
        public string? IssueContext { get; set; }

        /// <summary>
        /// The partition Key
        /// </summary>
        public string? PartitionKey { get; set; }

        /// <summary>
        /// The row key
        /// </summary>
        public string? RowKey { get; set; }

        /// <summary>
        /// The time stamp associated with the row entry
        /// </summary>
        public DateTimeOffset? Timestamp { get; set; }

        /// <summary>
        /// The etag associated with the row entry
        /// </summary>
        public ETag ETag { get; set; }
    }
}
