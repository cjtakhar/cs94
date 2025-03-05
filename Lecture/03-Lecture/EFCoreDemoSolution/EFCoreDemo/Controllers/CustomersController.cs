using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using EFCoreDemo.Common;
using EFCoreDemo.CustomSettings;
using EFCoreDemo.Data;
using EFCoreDemo.DataTransferObjects;
using EFCoreDemo.Models;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EFCoreDemo.ExtensionMethods;

namespace EFCoreDemo.Controllers
{
    public enum SortOrder
    {
        Ascending = 0,
        Descending = 1
    }

    /// <summary>
    /// Provides implementation for the customers resource
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    [Produces("application/json")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        /// <summary>
        /// The get customer by identifier route
        /// </summary>
        private const string GetCustomerByIdRoute = "GetCustomerByIdRoute";

        /// <summary>
        /// The database context
        /// </summary>
        private readonly MyDatabaseContext _context;
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
        /// Initializes a new instance of the <see cref="CustomersController"/> class.
        /// </summary>
        public CustomersController(ILogger<CustomersController> logger,
                                   MyDatabaseContext context,
                                   CustomerLimits customerLimits,
                                   TelemetryClient telemetryClient) // DEMO SETTINGS STEP 6
        {
            _logger = logger;
            _context = context;
            _telemetryClient = telemetryClient;

            // DEMO Step 7
            _customerLimits = customerLimits;

        }

        /// <summary>
        /// Gets the specified customer resource based on the id parameter.
        /// </summary>
        /// <param name="id">The customer's id.</param>
        /// <returns>The Customer resource</returns>
        /// <remarks>
        /// Demo Notes:
        /// An Id of 0 will generate a Server Error result.
        /// An Id of 42 will generate a Bad Request result</remarks>
        [HttpGet]
        [ProducesResponseType(typeof(CustomerResult), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(void), 500)]
        [Route("api/v1/customers/{id}", Name = GetCustomerByIdRoute)]
        public async Task<IActionResult> GetCustomerById(Guid id)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                // Retrieve the note by id and eagerly load its tags.
                CustomerWithAddressResult? customerWithAddressResult = await _context.Customers
                    .AsNoTracking()
                    .Where(n => n.Id == id)
                    .Include(n => n.Addresses)
                    .Select(n => new CustomerWithAddressResult
                    {
                        Age = n.Age,
                        EmailAddress = n.EmailAddress ?? "",
                        Id = n.Id,
                        Name = n.Name ?? "",
                        AddressList = n.Addresses.ToAddressResult()
                    })
                    .FirstOrDefaultAsync();

                if (customerWithAddressResult == null)
                {
                    _logger.LogInformation(LoggingEvents.GetItem, "CustomerController Customer id:[{id}] was not found.", id);
                    return NotFound();
                }

                return new ObjectResult(customerWithAddressResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.InternalError, ex, "CustomerController Get Customer id:[{id}] caused an internal error.", id);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
            finally
            {
                stopwatch.Stop();
                LogTimeToAppInsights(stopwatch);
            }
        }

        /// <summary>
        /// Gets the list of customers
        /// </summary>
        /// <returns>The list of customers</returns>
        /// <param name="minAge">The minimum age or null for default value which is 0</param>
        /// <param name="maxAge">The maximum age or null for default value which is max int</param>
        /// <param name="sortOrder">Ascending or Descending or null for default value which is Ascending</param>
        /// <param name="zipCode">The zip code or null for default value which is null</param>
        [HttpGet]
        [ProducesResponseType(typeof(List<CustomerResult>), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(void), 500)]
        [Route("api/v1/customers")]
        public IActionResult GetAllCustomers(int? minAge = null, int? maxAge = null, string? zipCode = null, SortOrder? sortOrder = null)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                IQueryable<Customer> query = _context.Customers;
                // SELECT * FROM Customers

                if (minAge != null)
                {
                    query = _context.Customers.Where(c => c.Age >= minAge);
                    // SELECT * FROM Customers C WHERE C.Age >= minAge
                }
                if (maxAge != null)
                {
                    query = query.Where(c => c.Age <= maxAge);
                    // SELECT * FROM Customers C WHERE C.Age >= minAge AND C.Age <= maxAge
                }

                if (!string.IsNullOrEmpty(zipCode))
                {
                    query = query.Where(c => c.Addresses.Any(a => a.ZipCode == zipCode));
                    // SELECT * FROM Customers C WHERE C.Addresses.ZipCode == zipCode
                }

                if ((sortOrder ?? SortOrder.Ascending) == SortOrder.Ascending)
                {
                    query = query.OrderBy(c => c.Age);
                    // SELECT * FROM Customers C WHERE C.Age >= minAge AND C.Age <= maxAge ORDER BY c.Age asc
                }
                else
                {
                    query = query.OrderByDescending(c => c.Age);
                    // SELECT * FROM Customers C WHERE C.Age >= minAge AND C.Age <= maxAge ORDER BY c.Age desc
                }

                // Query is executed here against Azure SQL via the ToList() method call.
                List<CustomerResult> customers = query.Select(c => new CustomerResult(c)).ToList();

                // An actual Query Looks Like this:
                //SELECT [c].[Id], [c].[Age], [c].[EmailAddress], [c].[Name]\r\nFROM [Customer] AS [c]\r\nWHERE ([c].[Age] >= @__minAge_0) AND ([c].[Age] <= @__maxAge_1)\r\nORDER BY [c].[Age]

                return new ObjectResult(customers);
            }
            finally
            {
                stopwatch.Stop();

                LogTimeToAppInsights(stopwatch);
            }
        }

        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(void), 500)]
        [Route("api/v1/customers/{id}")]
        [HttpPatch]
        public IActionResult PatchCustomer(Guid id, [FromBody] CustomerPatchPayload customerPatchPayload)
        {
            // LINQ Example
            Customer? customerEntity = (from c in _context.Customers where c.Id == id select c).SingleOrDefault();

            if (customerEntity == null)
            {
                return NotFound();
            }

            customerEntity.Name = customerPatchPayload.Name ?? customerEntity.Name;
            customerEntity.EmailAddress = customerPatchPayload.EmailAddress ?? customerEntity.EmailAddress;
            customerEntity.Age = customerPatchPayload.Age ?? customerEntity.Age;

            _context.Customers.Update(customerEntity);
            _context.SaveChanges();
            return NoContent();

        }


        /// <summary>
        /// Updates or creates the customer
        /// </summary>
        /// <param name="id">The identifier of the customer.</param>
        /// <param name="customerUpdatePayload">The customer.</param>
        /// <returns>An IAction result indicating HTTP 204 no content if success update
        /// HTTP 201 if successful create
        /// otherwise BadRequest if the input is not valid.</returns>
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(CustomerResult), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(void), 500)]
        [Route("api/v1/customers/{id}")]
        [HttpPut]
        public IActionResult UpdateOrCreateCustomer(Guid id, [FromBody] CustomerUpdatePayload customerUpdatePayload)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                if (ModelState.IsValid)
                {
                    // Method Chaining Example
                    //Customer customerEntity = _context.Customers.Where(c => c.Id == id).Select(c => c).SingleOrDefault();

                    // LINQ Example
                    Customer? customerEntity = (from c in _context.Customers where c.Id == id select c).SingleOrDefault();

                    // if the customer entity was not found create the entity at the ID specified by the caller
                    if (customerEntity == null)
                    {
                        // First verify that there are not the max customers already
                        if (!CanAddMoreCustomers())
                        {
                            return StatusCode((int)HttpStatusCode.Forbidden, $"Customer limit reached MaxCustomers: [{_customerLimits.MaxCustomers}]");
                        }

                        customerEntity = new Customer()
                        {
                            Id = id,
                            Name = customerUpdatePayload.Name,
                            Age = customerUpdatePayload.Age,
                            EmailAddress = customerUpdatePayload.EmailAddress,
                            Addresses = new List<Address>()
                        };

                        // Add the addresses for the customer
                        foreach (var address in customerUpdatePayload.AddressList)
                        {
                            customerEntity.Addresses.Add(new Address()
                            {
                                CustomerId = id,
                                Line1 = address.Line1,
                                Line2 = address.Line2,
                                City = address.City,
                                State = address.State,
                                ZipCode = address.ZipCode
                            });
                        }

                        // Tell Entity Framework to add the customer entity
                        _context.Customers.Add(customerEntity);

                        // Save the changes to the database
                        _context.SaveChanges();

                        return CreatedAtRoute(GetCustomerByIdRoute, new { id = customerEntity.Id }, new CustomerResult(customerEntity));

                    }

                    // Delete the existing customer addresses for this customer
                    IQueryable<Address> addressesToDelete = _context.Addresses.Where(t => t.CustomerId == customerEntity.Id);
                    _context.Addresses.RemoveRange(addressesToDelete);

                    // Add the new addresses
                    customerEntity.Addresses = new List<Address>();
                    foreach (var address in customerUpdatePayload.AddressList)
                    {
                        customerEntity.Addresses.Add(new Address()
                        {
                            CustomerId = id,
                            Line1 = address.Line1,
                            Line2 = address.Line2,
                            City = address.City,
                            State = address.State,
                            ZipCode = address.ZipCode
                        });
                    }
                    // Update the entity specified by the caller.
                    customerEntity.Age = customerUpdatePayload.Age;
                    customerEntity.EmailAddress = customerUpdatePayload.EmailAddress;
                    customerEntity.Name = customerUpdatePayload.Name;

                    // Tell Entity Framework to update the customer entity
                    _context.SaveChanges();
                }
                else
                {
                    return BadRequest(ModelState);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.InternalError, ex, "CustomerController Put Customer id:[{id}] caused an internal error.", id);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
            finally
            {
                stopwatch.Stop();

                LogTimeToAppInsights(stopwatch);
            }
            return NoContent();
        }

        /// <summary>
        /// Determines whether this more customers can be added.
        /// </summary>
        /// <returns>
        ///   true if more customers can be added false if not
        /// </returns>
        private bool CanAddMoreCustomers()
        {
            long totalCustomers = (from c in _context.Customers select c).Count();

            // DEMO SETTINGS:
            if (_customerLimits.MaxCustomers > totalCustomers)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Deletes the customer
        /// </summary>
        /// <param name="id">The identifier of the customer.</param>
        /// <returns>An IAction result indicating HTTP 204 no content if success otherwise BadRequest if the input is not valid.</returns>
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(void), 500)]
        [Route("api/v1/customers/{id}")]
        [HttpDelete]
        public IActionResult DeleteCustomerById(Guid id)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                Customer? dbCustomer = (from c in _context.Customers where c.Id == id select c).SingleOrDefault();

                if (dbCustomer == null)
                {
                    _logger.LogInformation(LoggingEvents.UpdateItem, "CustomerController Customer id:[{id}] was not found.", id);
                    return NotFound();
                }

                _context.Customers.Remove(dbCustomer);

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.InternalError, ex, "CustomerController Put Customer id:[{id}] caused an internal error.", id);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
            finally
            {
                stopwatch.Stop();

                LogTimeToAppInsights(stopwatch);
            }

            return NoContent();
        }

        /// <summary>
        /// Creates the customer
        /// </summary>
        /// <param name="customerCreatePayload">The customer.</param>
        /// <returns>An IAction result indicating HTTP 201 created if success otherwise BadRequest if the input is not valid.</returns>
        [ProducesResponseType(typeof(CustomerResult), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(void), 500)]
        [Route("api/v1/customers")]
        [HttpPost]
        public IActionResult CreateCustomer([FromBody] CustomerCreatePayload customerCreatePayload)
        {
            Customer customerEntity = new Customer();
            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                if (ModelState.IsValid)
                {
                    // First verify that there are not the max customers allready
                    if (!CanAddMoreCustomers())
                    {
                        ProblemDetails problemDetails = new ProblemDetails()
                        {
                            Status = StatusCodes.Status403Forbidden,
                            Title = "Customer limit reached",
                            Detail = $"Customer limit reached MaxCustomers: [{_customerLimits.MaxCustomers}]"
                        };
                        return StatusCode((int)HttpStatusCode.Forbidden, problemDetails);
                    }

                    customerEntity.Age = customerCreatePayload.Age;
                    customerEntity.EmailAddress = customerCreatePayload.EmailAddress;
                    customerEntity.Name = customerCreatePayload.Name;

                    // Tell entity framework to add the address entity
                    _context.Customers.Add(customerEntity);

                    int result = _context.SaveChanges();
                }
                else
                {
                    return BadRequest(ModelState);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.InternalError, ex, "CustomerController Post CustomerEntity([{customerEntity}]) CustomerCreatePayload([{customerCreatePayload}] caused an internal error.", customerEntity, customerCreatePayload);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
            finally
            {
                stopwatch.Stop();

                LogTimeToAppInsights(stopwatch);
            }
            return CreatedAtRoute(GetCustomerByIdRoute, new { id = customerEntity.Id }, new CustomerResult(customerEntity));
        }

        private void LogTimeToAppInsights(Stopwatch stopwatch, [CallerMemberName] string operationName = "")
        {
            _telemetryClient.TrackEvent("ACustomerRetrieved",
                        properties: new Dictionary<string, string>()
                        {
                                { "TotalSQLTime", stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff") },
                                { "TotalSQLTimeMilliseconds", stopwatch.ElapsedMilliseconds.ToString() },
                                { "OperationName", operationName }
                        },
                        metrics: new Dictionary<string, double>()
                        {
                                { "TotalSQLTimeMilliseconds", stopwatch.ElapsedMilliseconds }
                        });
        }

    }
}
