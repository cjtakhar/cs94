using EFCoreDemo.Models;

namespace EFCoreDemo.DataTransferObjects
{
    /// <summary>
    /// Defines the public facing customer attributes
    /// </summary>
    public class CustomerResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerResult"/> class using a Customer as input.
        /// </summary>
        /// <param name="customer">The customer.</param>
        public CustomerResult(Customer customer)
        {
            Id = customer.Id;
            Name = customer.Name;
            EmailAddress = customer.EmailAddress;
            Age = customer.Age;
        }
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
    }
}
