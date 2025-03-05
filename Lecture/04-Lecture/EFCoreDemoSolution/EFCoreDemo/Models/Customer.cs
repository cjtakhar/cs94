using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCoreDemo.Models
{
    /// <summary>
    /// The Customer entity.
    /// </summary>
    /// <remarks>Step 3</remarks>
    public class Customer
    {
        /// <summary>
        /// Gets or sets the customer identifier.
        /// </summary>
        /// <value>The identifier.</value>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // DatabaseGeneratedOption.Identity causes entity framework to generate the GUID value
        public Guid Id { get; set; }
        /// <summary>
        /// Gets or sets the customer's name.
        /// </summary>
        /// <value>The name.</value>
        [Required]
        [StringLength(30)]
        public string? Name { get; set; }
        /// <summary>
        /// Gets or sets the customer's email address.
        /// </summary>
        /// <value>The email address.</value>
        public string? EmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the age.
        /// </summary>
        /// <value>The age.</value>
        [Required]
        [Range(21, 120)]
        public int Age { get; set; }

        /// <summary>
        /// Gets or sets the addresses.
        /// </summary>
        /// <value>
        /// The addresses.
        /// </value>
        public ICollection<Address> Addresses { get; set; } = null!;


        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"Id=[{Id}] Name=[{Name}], Age=[{Age}], EmailAddress=[{EmailAddress}]";
        }

    }
}
