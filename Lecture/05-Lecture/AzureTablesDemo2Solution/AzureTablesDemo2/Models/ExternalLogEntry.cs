
using Azure;
using Azure.Data.Tables;

namespace AzureTablesDemo2.Models
{
    /// <summary>
    /// Defines the input payload for an external log entry
    /// </summary>
    public class ExternalLogEntryInput
    {
        /// <summary>
        /// Gets or sets the details of the issue.
        /// </summary>
        /// <value>The details of the issue.</value>
        public string? Details { get; set; }


        /// <summary>
        /// Gets or sets the date the issue was recorded.
        /// </summary>
        /// <value>The date the issue was recorded.</value>
        public DateTime EntryDate { get; set; }

        /// <summary>
        /// Gets or sets the event identifier associated with the issue.
        /// </summary>
        /// <value>The event identifier associated with the issue</value>
        public int EventId { get; set; }
    }

    /// <summary>
    /// Describes the full result payload for an external log entry
    /// </summary>
    public class ExternalLogEntry : ITableEntity
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalLogEntry"/> class.
        /// </summary>
        public ExternalLogEntry()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalLogEntry"/> class.
        /// </summary>
        /// <param name="logEntryInput">The log entry input to initialize the instance with.</param>
        public ExternalLogEntry(ExternalLogEntryInput logEntryInput)
        {
            Details = logEntryInput.Details;
            EntryDate = logEntryInput.EntryDate;
            EventId = logEntryInput.EventId;
        }

        /// <summary>
        /// Gets or sets the details of the issue.
        /// </summary>
        /// <value>The details of the issue.</value>
        public string? Details { get; set; }


        /// <summary>
        /// Gets or sets the date the issue was recorded.
        /// </summary>
        /// <value>The date the issue was recorded.</value>
        public DateTime EntryDate { get; set; }

        /// <summary>
        /// Gets or sets the event identifier associated with the issue.
        /// </summary>
        /// <value>The event identifier associated with the issue</value>
        public int EventId { get; set; }

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
