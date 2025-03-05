namespace AzureBlobDemo
{
    /// <summary>
    /// Defines the blob response providing only the name
    /// </summary>
    public class BlobName
    {
        /// <summary>
        /// Gets or sets the blob's  container.
        /// </summary>
        /// <value>The blob's container.</value>
        public string? Container { get; set; }
        /// <summary>
        /// Gets or sets the blob name.
        /// </summary>
        /// <value>The blob's name.</value>
        public string? Name { get; set; }

        /// <summary>
        /// Custom metadata entry named "CustomMetadata" associated with the blob
        /// </summary>
        /// <value>The value of the custom metadata entry named CustomMetadata</value>
        public string? CustomMetadata  { get; set; }

        /// <summary>
        /// The date time the blob was created
        /// </summary>
        public DateTimeOffset? CreatedDate { get; set; }

        /// <summary>
        /// The date time the blob was last modified
        /// </summary>
        public DateTimeOffset? LastModifiedDate { get; set; }

        /// <summary>
        /// The length of the blob
        /// </summary>
        public long Length { get; set; }
    }
}

