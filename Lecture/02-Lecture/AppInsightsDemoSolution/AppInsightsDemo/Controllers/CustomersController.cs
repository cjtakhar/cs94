using Microsoft.AspNetCore.Mvc;
using AppInsightsDemo.PublicModels;
using AppInsightsDemo.CustomSettings;
using Microsoft.ApplicationInsights;

namespace AppInsightsDemo.Controllers
{

    public partial class CustomersController : ControllerBase
    {
        /// <summary>
        /// The name of the route used to return the list of customers
        /// </summary>
        private const string GetCustomersRouteName = "GetCustomers";

        /// <summary>
        /// The in memory list of customers
        /// </summary>
        /// <remarks>This is not thread safe</remarks>
        static private Dictionary<int, Customer> _customers;

        /// <summary>
        /// The App Insights Telemetry Client
        /// </summary>
        private readonly TelemetryClient _telemetryClient;

        /// <summary>
        /// Logger instance
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// The customer limits settings
        /// </summary>
        private readonly CustomerLimits _customerLimits;

        /// <summary>
        /// Constructor, initialize the default set of customers
        /// </summary>
        static CustomersController()
        {
            // Create a default set of customers for the demo
            _customers = new Dictionary<int, Customer>()
            {
                { 1, new Customer() {
                    Id = 1,
                    Name="Acme Auto",
                    Phone = "555-555-1212",
                    CustomerAddress = new Address()
                    {
                        Address1 = "1 Main Street",
                        Address2 = "",
                        City= "Springfield",
                        State = "MA",
                        ZipCode = "12345"
                    }
                }
                },
                {
                    2, new Customer()
                    {
                     Id = 2,
                     Name="Blue Moon Software",
                     Phone="203-444-1121",
                     CustomerAddress = new Address()
                     {
                       Address1 = "5 Avenue",
                       Address2 = "",
                       City= "New York",
                       State = "NY",
                       ZipCode = "42345"
                     }
                    }
                },
                {
                    3,new Customer()
                    {
                        Id = 3,
                        Name="Matrix Simulations",
                        Phone="777-444-1111",
                        CustomerAddress = new Address()
                        {
                            Address1 = "1 Unimatrix Street",
                            Address2 = "",
                            City= "Matrix",
                            State = "II",
                            ZipCode = "55555"
                     }
                    }
                }
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomersController"/> class.
        /// </summary>
        public CustomersController(ILogger<CustomersController> logger,
                                   CustomerLimits customerLimits,
                                   TelemetryClient telemetryClient)
        {
            _telemetryClient = telemetryClient;
            _logger = logger;

            // DEMO SETTINGS STEP 5
            _customerLimits = customerLimits;

        }
    }
}
