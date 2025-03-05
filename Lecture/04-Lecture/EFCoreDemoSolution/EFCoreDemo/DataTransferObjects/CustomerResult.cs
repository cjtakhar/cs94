namespace EFCoreDemo.DataTransferObjects
{
    /// <summary>
    /// Defines the public facing customer attributes
    /// </summary>
    public class CustomerWithAddressResult
    {

        /// <summary>
        /// Gets or sets the customer identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the customer's name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the customer's email address.
        /// </summary>
        /// <value>The email address.</value>
        public string EmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the age.
        /// </summary>
        /// <value>The age.</value>
        public int Age { get; set; }

        /// <summary>
        /// The list of addresses associated with the customer
        /// </summary>
        public List<AddressResult> AddressList { get; set; }
    }
}
