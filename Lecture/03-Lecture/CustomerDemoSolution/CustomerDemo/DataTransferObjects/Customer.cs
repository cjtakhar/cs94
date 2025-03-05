using System.Diagnostics;

namespace CustomerDemo.DataTransferObjects
{
    /// <summary>
    /// The Customer entity.
    /// </summary>
    [DebuggerDisplay("Id: [{Id}] Name: [{Name}]")]
    public class Customer
    {
        /// <summary>
        /// Gets or sets the customer identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the customer's name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the customer's email address.
        /// </summary>
        /// <value>The email address.</value>
        public string EmailAddress { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the age.
        /// </summary>
        /// <value>The age.</value>
        public int Age { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the customer is active
        /// </summary>
        /// <value>
        ///   <c>true</c> if the customer is active otherwise, <c>false</c>.
        /// </value>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or Sets the date of birth of the customer
        /// </summary>
        /// <value>
        /// The date of birth
        /// </value>
        public DateTime BirthDate { get; set; }
    }
}