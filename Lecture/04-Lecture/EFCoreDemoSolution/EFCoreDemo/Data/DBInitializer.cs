using EFCoreDemo.Models;

namespace EFCoreDemo.Data
{
    /// <summary>
    /// Initializes (seeds) the database with data
    /// </summary>
    /// <remarks>Step 7</remarks>
    public class DbInitializer
    {
        /// <summary>
        /// Initializes the specified context with data
        /// </summary>
        /// <param name="context">The context.</param>
        public static async Task InitializeAsync(MyDatabaseContext context)
        {
            // Check to see if there is any data in the customer table
            if (context.Customers.Any())
            {
                // Customer table has data, nothing to do here
                return;
            }

            // Create some data
            Customer[] customers = new Customer[]
            {
                new Customer() { Name = "Sam Flynn", EmailAddress = "sam.flynn@encorp.com", Age = 25,
                Addresses = new List<Address>() {
                                                    new Address()
                                                        {
                                                            Line1 = "1 Main St",
                                                            City = "Boston",
                                                            State = "MA",
                                                            ZipCode = "01234"
                                                        },
                                                    new Address()
                                                        {
                                                            Line1 = "5 Main St",
                                                            City = "Tampa",
                                                            State = "FL",
                                                            ZipCode = "21234"
                                                        }

                                                }
                },
                new Customer() { Name = "Captain Kirk", EmailAddress = "captain.kirk@ussenterprise.com", Age=34,
                Addresses = new List<Address>() {
                                                    new Address()
                                                        {
                                                            Line1 = "5 Post Road",
                                                            City = "Westwood",
                                                            State = "MA",
                                                            ZipCode = "01244"
                                                        },
                                                    new Address()
                                                        {
                                                            Line1 = "15 Fields Ln",
                                                            City = "Dallas",
                                                            State = "TX",
                                                            ZipCode = "91234"
                                                        }
                                                }
                },
                new Customer() { Name = "Mr Spock", EmailAddress = "mr.spock@ussenterprise.com", Age=99 },
                new Customer() { Name = "Kiera Cameron", EmailAddress = "kiera.cameron@vancouverpolice.gov", Age=26 }
            };

            // Add the data to the in memory model
            foreach (Customer customer in customers)
            {
                context.Customers.Add(customer);
            }

            // Commit the changes to the database
            await context.SaveChangesAsync();

            // The Customers added now are populated with their Ids
            Console.WriteLine("Customers Added:");
            foreach (Customer customer in context.Customers)
            {
                Console.WriteLine($"\tCustomer Id: {customer.Id} Name: {customer.Name}" );
            }
        }
    }
}
