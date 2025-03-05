using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCoreDemo.Models
{
    /// <summary>
    /// The Address entity.
    /// </summary>
    public class Address
    {

        /// <summary>
        /// Gets or sets the address identifier.
        /// </summary>
        /// <value>The address identifier.</value>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? AddressId { get; set; }

        /// <summary>
        /// Gets or sets the line 1 of the address
        /// </summary>
        /// <value>Line 1 of the address.</value>
        [Required]
        [StringLength(200)]
        public string? Line1 { get; set; }

        /// <summary>
        /// Gets or sets the line 2 of the address
        /// </summary>
        /// <value>Line 2 of the address.</value>
        [StringLength(200)]
        public string? Line2 { get; set; }

        /// <summary>
        /// Gets or sets the city 
        /// </summary>
        /// <value>Line city.</value>
        [StringLength(100)]
        public string? City { get; set; }

        /// <summary>
        /// Gets or sets the state 
        /// </summary>
        /// <value>State.</value>
        [StringLength(100)]
        public string? State { get; set; }

        /// <summary>
        /// Gets or sets the state 
        /// </summary>
        /// <value>Zip code.</value>
        [StringLength(10)]
        public string? ZipCode { get; set; }

        /// <summary>
        /// Gets or sets the customer id.
        /// </summary>
        /// <value>
        /// The customer id.
        /// </value>
        public Guid CustomerId { get; set; }

        /// <summary>
        /// Navigation property back to the Customer
        /// </summary>
        public Customer Customer { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"AddressId=[{AddressId}], Line1=[{Line1}], Line2=[{Line2}], City=[{City}], State=[{State}], Line2=[{ZipCode}]";
        }

    }
}
