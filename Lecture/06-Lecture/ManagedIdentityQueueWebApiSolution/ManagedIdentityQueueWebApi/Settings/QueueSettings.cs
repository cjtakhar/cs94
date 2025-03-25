namespace ManagedIdentityQueueWebApi.Settings
{
    /// <summary>
    /// Settings related to the queue
    /// </summary>
    public class QueueSettings
    {
        /// <summary>
        /// The connection string to the string queue
        /// </summary>
        public string? StringQueueConnectionString { get; set; }

        /// <summary>
        /// The connection string to the json queue
        /// </summary>
        public string? JsonQueueConnectionString { get; set; }
    }
}
