namespace AppInsightsDemo.PublicModels
{
    public class AddressCreatePayload
    {
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }

        internal Address ToAddress()
        {
            return new Address()
            {
                Address1 = this.Address1,
                Address2 = this.Address2,
                City = this.City,
                State = this.State,
                ZipCode = this.State
            };
        }
    }
}
