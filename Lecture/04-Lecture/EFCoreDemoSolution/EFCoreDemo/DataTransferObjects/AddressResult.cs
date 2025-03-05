namespace EFCoreDemo.DataTransferObjects
{
    /// <summary>
    /// Defines the public facing address attributes
    /// </summary>
    public class AddressResult
    {
        /// <summary>
        /// Gets or sets the address identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the line 1 of the address
        /// </summary>
        /// <value>Line 1 of the address.</value>
        public string? Line1 { get; set; }

        /// <summary>
        /// Gets or sets the line 2 of the address
        /// </summary>
        /// <value>Line 2 of the address.</value>
        public string? Line2 { get; set; }

        /// <summary>
        /// Gets or sets the city 
        /// </summary>
        /// <value>Line city.</value>
        public string? City { get; set; }

        /// <summary>
        /// Gets or sets the state 
        /// </summary>
        /// <value>State.</value>
        public string? State { get; set; }

        /// <summary>
        /// Gets or sets the state 
        /// </summary>
        /// <value>Zip code.</value>
        public string? ZipCode { get; set; }
    }
}
