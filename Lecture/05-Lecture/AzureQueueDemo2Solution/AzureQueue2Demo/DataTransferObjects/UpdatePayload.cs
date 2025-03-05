namespace AzureQueueDemo.DataTransferObjects
{
    /// <summary>
    /// The update payload for the queued message
    /// </summary>
    public class UpdatePayload
    {
        /// <summary>
        /// Gets or sets the visibility in seconds.
        /// </summary>
        /// <value>
        /// The visibility in seconds.
        /// </value>
        public int? VisibilityInSeconds { get; set; }

        /// <summary>
        /// Gets or sets the message text.
        /// </summary>
        /// <value>
        /// The message text.
        /// </value>
        public string MessageText { get; set; }
    }
}
