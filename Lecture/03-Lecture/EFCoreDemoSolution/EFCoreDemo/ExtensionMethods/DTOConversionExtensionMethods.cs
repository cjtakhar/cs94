namespace EFCoreDemo.ExtensionMethods
{
    public static class DTOConversionExtensionMethods
    {
        public static List<DataTransferObjects.AddressResult> ToAddressResult(this ICollection<Models.Address>? addresses)
        {
            if (addresses == null)
            {
                return [];
            }

            return addresses.Select(a => new DataTransferObjects.AddressResult
            {
                Id = a.AddressId ?? 0,
                Line1 = a.Line1,
                Line2 = a.Line2,
                City = a.City,
                State = a.State,
                ZipCode = a.ZipCode
            }).ToList();
        }
    }
}
