using System.ComponentModel.DataAnnotations;

namespace EFCoreDemo.DataTransferObjects
{
    /// <summary>
    /// The Customer creation shape.
    /// </summary>
    [CustomValidation(typeof(CustomerCreatePayload), "ValidateNameAndEmail")]
    public class CustomerCreatePayload
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
        /// Validates the name and email.
        /// </summary>
        /// <param name="customerCreatePayload">The customer.</param>
        /// <param name="ctx">The context which contains the actual value from the field that the custom validator is validating.</param>
        /// <returns>ValidationResult.</returns>
        public static ValidationResult? ValidateNameAndEmail(CustomerCreatePayload customerCreatePayload, ValidationContext ctx)
        {
            // Verify that the email address contains either the first or last name
            string[] nameParts = customerCreatePayload.Name.ToLowerInvariant().Split(' ');

            if (nameParts == null || nameParts.Length < 1)
            {
                return new ValidationResult("Missing name", new List<string> { "Name" });
            }

            if (customerCreatePayload.EmailAddress.ToLowerInvariant().Contains(nameParts[0]))
            {
                return ValidationResult.Success;
            }

            if (nameParts.Length > 1 && customerCreatePayload.EmailAddress.ToLowerInvariant().Contains(nameParts[1]))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult("Email must contain first or last name",
                                         new List<string> { "Name", "EmailAddress" });
        }


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
