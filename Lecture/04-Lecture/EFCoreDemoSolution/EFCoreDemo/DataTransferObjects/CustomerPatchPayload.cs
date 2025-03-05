using System.ComponentModel.DataAnnotations;


namespace EFCoreDemo.DataTransferObjects
{
    /// <summary>
    /// The Customer creation shape.
    /// </summary>
    public class CustomerPatchPayload
    {
        public string? Name { get; set; }

        public string? EmailAddress { get; set; }

        [Range(21, 120)]
        public int? Age { get; set; }
    }
}
