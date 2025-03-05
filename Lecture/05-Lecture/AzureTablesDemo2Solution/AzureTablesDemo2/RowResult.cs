namespace AzureTables2
{
    /// <summary>
    /// The result data associated with a row creation 
    /// </summary>
    public class RowResult
    {
        /// <summary>
        /// Gets or sets the row key.
        /// </summary>
        /// <value>The row key.</value>
        public string RowKey { get; set; }
        /// <summary>
        /// Gets or sets the partition key.
        /// </summary>
        /// <value>The partition key.</value>
        public string PartitionKey { get; set; }
        /// <summary>
        /// Gets or sets the etag.
        /// </summary>
        /// <value>The etag.</value>
        public string Etag { get; set; }

    }
}
