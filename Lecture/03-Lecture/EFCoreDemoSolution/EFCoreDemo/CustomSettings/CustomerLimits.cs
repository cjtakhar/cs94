namespace EFCoreDemo.CustomSettings
{
    /// <summary>
    /// Defines the customer limit settings
    /// </summary>
    /// <remarks>
    /// DEMO SETTINGS Step 2
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
        public int MaxCustomers { get; set; } = 25;
    }

}
