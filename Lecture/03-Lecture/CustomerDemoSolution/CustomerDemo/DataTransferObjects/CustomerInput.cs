using System.ComponentModel.DataAnnotations;

namespace CustomerDemo.DataTransferObjects
{
    [CustomValidation(typeof(CustomerInput), "ValidateNameAndEmail")]
    public class CustomerInput
    {
        /// <summary>
        /// Gets or sets the customer's name.
        /// </summary>
        /// <value>The name.</value>
        [Required]
        [StringLength(30, ErrorMessage = "1")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the customer's email address.
        /// </summary>
        /// <value>The email address.</value>
        [EmailAddress(ErrorMessage = "2")]
        public string EmailAddress { get; set; } = string.Empty;

        /// <summary>As
        /// Gets or sets the age.
        /// </summary>
        /// <value>The age.</value>
        [Required]
        [Range(21, 120, ErrorMessage = "3")]
        public int Age { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the customer is active
        /// </summary>
        /// <value>
        ///   <c>true</c> if the customer is active otherwise, <c>false</c>.
        /// </value>
        [Required(ErrorMessage = "4")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Validates the name and email.
        /// </summary>
        /// <param name="customer">The customer.</param>
        /// <param name="ctx">The context which contains the actual value from the field that the custom validator is validating.</param>
        /// <returns>ValidationResult.</returns>
        public static ValidationResult? ValidateNameAndEmail(CustomerInput customer, ValidationContext ctx)
        {
            if (string.IsNullOrWhiteSpace(customer?.Name))
            {
                return new ValidationResult("5", new List<string> { "Name" });
            }
            // Verify that the email address contains either the first or last name
            string[] nameParts = customer.Name.ToLowerInvariant().Split(' ');

            if (nameParts == null || nameParts.Length < 1)
            {
                return new ValidationResult("5", new List<string> { "Name" });
            }

            string[] emailAddressParts = customer.EmailAddress.ToLowerInvariant().Split("@");

            if (emailAddressParts == null || emailAddressParts.Length < 1)
            {
                return new ValidationResult(((int)ErrorCode.InvalidEmailAddress).ToString(), new List<string> { "EmailAddress" });
            }

            if (emailAddressParts[0].ToLowerInvariant().Contains(nameParts[0]))
            {
                return ValidationResult.Success;
            }

            if (nameParts.Length > 1 && emailAddressParts[0].ToLowerInvariant().Contains(nameParts[1]))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult("6",
                                         new List<string> { "EmailAddress" });
        }
    }
}
