using System.ComponentModel.DataAnnotations;


namespace EFCoreDemo.DataTransferObjects
{
    /// <summary>
    /// The Customer creation shape.
    /// </summary>
    public class CustomerUpdatePayload
    {
        /// <summary>
        /// Gets or sets the customer's name.
        /// </summary>
        /// <value>The name.</value>
        [Required]
        [StringLength(30)]
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
        [Required]
        [Range(21, 120)]
        public int Age { get; set; }

        /// <summary>
        /// The list of addresses associated with the customer
        /// </summary>
        public List<AddressCreatePayload> AddressList { get; set; } = new List<AddressCreatePayload>();

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"Name=[{Name}], Age=[{Age}], EmailAddress=[{EmailAddress}]";
        }
    }
}
