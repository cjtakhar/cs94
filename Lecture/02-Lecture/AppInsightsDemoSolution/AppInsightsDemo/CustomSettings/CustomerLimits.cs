namespace AppInsightsDemo.CustomSettings
{
    /// <summary>
    /// Defines the customer limit settings
    /// </summary>
    /// <remarks>    
    /// </remarks>
    public class CustomerLimits
    {
        /// <summary>
        /// Gets or sets the maximum number of customers.
        /// </summary>
        /// <value>
        /// The maximum number of customers.
        /// </value>
        /// <remarks>Setting a default max customers if non provided in config</remarks>
        public int MaxCustomers { get; set; } = 10;

        /// <summary>
        /// Gets or sets the maximum number of customers to add at a time.
        /// </summary>
        /// <value>
        /// The maximum number of customers to add at a time.
        /// </value>
        /// <remarks>Setting a default max batch customers if non provided in config</remarks>
        public int MaxBatchCustomers { get; set; } = 3;
    }

}
