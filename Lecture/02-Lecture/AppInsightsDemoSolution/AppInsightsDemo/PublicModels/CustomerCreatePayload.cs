namespace AppInsightsDemo.PublicModels
{
    public class CustomerCreatePayload
    {
        public string Name { get; set; }

        public string Phone { get; set; }

        public AddressCreatePayload CustomerAddress { get; set; }

        internal Customer ToCustomer(int id)
        {
            return new Customer
            {
                Id = id,
                Name = this.Name,
                Phone = this.Phone,
                CustomerAddress = this.CustomerAddress.ToAddress()
            };
        }
    }
}
