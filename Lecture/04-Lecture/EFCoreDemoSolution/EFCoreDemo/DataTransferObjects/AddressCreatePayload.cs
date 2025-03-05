namespace EFCoreDemo.DataTransferObjects
{

    /// <summary>
    /// Defines the public facing address attributes for address creation
    /// </summary>
    public class AddressCreatePayload
    {
        /// <summary>
        /// Gets or sets the line 1 of the address
        /// </summary>
        /// <value>Line 1 of the address.</value>
        public string Line1 { get; set; }

        /// <summary>
        /// Gets or sets the line 2 of the address
        /// </summary>
        /// <value>Line 2 of the address.</value>
        public string Line2 { get; set; }

        /// <summary>
        /// Gets or sets the city 
        /// </summary>
        /// <value>Line city.</value>
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the state 
        /// </summary>
        /// <value>State.</value>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the state 
        /// </summary>
        /// <value>Zip code.</value>
        public string ZipCode { get; set; }
    }
}
