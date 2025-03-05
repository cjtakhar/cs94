namespace AppInsightsDemo
{
    /// <summary>
    /// The error information
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// The error number used if there are no customers provided in the batch create
        /// </summary>
        public const int NoCustomersProvided = 1;

        /// <summary>
        /// The error number used if there are to many customers
        /// </summary>
        public const int ToManyCustomers = 2;

        /// <summary>
        /// The error number used if there are to many customers in a batch
        /// </summary>
        public const int ToManyBatchCustomers = 3;

        /// <summary>
        /// The error number used if the customer was not found
        /// </summary>
        public const int CustomerNotFound = 4;

        /// <summary>
        /// The error number used if an internal error occured.
        /// </summary>
        public const int InternalError = -1;        

        /// <summary>
        /// The error number
        /// </summary>
        public int ErrorNumber { get; set; }
        
        /// <summary>
        /// A human readable error description in en-US
        /// </summary>
        public string? Description { get; set; }
    }
}
