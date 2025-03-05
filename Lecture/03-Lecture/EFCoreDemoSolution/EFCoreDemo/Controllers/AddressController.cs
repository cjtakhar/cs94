using EFCoreDemo.Common;
using EFCoreDemo.CustomSettings;
using EFCoreDemo.Data;
using EFCoreDemo.DataTransferObjects;
using EFCoreDemo.Models;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;

namespace EFCoreDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressController : ControllerBase
    {
        /// <summary>
        /// The get address by identifier route
        /// </summary>
        private const string GetAddressByIdRoute = "GetAddressByIdRoute";

        /// <summary>
        /// The get all addresses route
        /// </summary>
        public const string GetAllAddressesRoute = "GetAllAddressesRoute";

        /// <summary>
        /// The customer lookup by identifier custom telemetry key name
        /// </summary>
        private const string CustomerLookupByIdCustomTelemetryKeyName = "CustomerLookupById";

        /// <summary>
        /// Telemetry client instance
        /// </summary>
        private readonly TelemetryClient _telemetryClient;

        /// <summary>
        /// Logger instance
        /// </summary>
        private readonly ILogger _logger;
        private readonly MyDatabaseContext _context;
        private readonly CustomerLimits _customerLimits;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressController"/> class.
        /// </summary>
        /// <param name="logger">The logger to log for diagnostics</param>
        /// <param name="context">The database context</param>
        /// <param name="customerLimits">The limit of the number of customers</param>
        /// <param name="telemetryClient">The telemetry client used for diagnostics</param>
        public AddressController(ILogger<CustomersController> logger,
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
        /// Gets a list of all addresses
        /// </summary>        
        /// <returns></returns>
        [ProducesResponseType(typeof(AddressResult[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(void), 500)]
        [HttpGet]
        [Route("api/v1/addresses", Name = GetAllAddressesRoute)]
        public IActionResult GetAllAddresses()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                AddressResult[] result = (from a in _context.Addresses
                                          select new AddressResult()
                                          {
                                              Id = a.AddressId ?? -1,
                                              Line1 = a.Line1,
                                              Line2 = a.Line2,
                                              City = a.City,
                                              State = a.State,
                                              ZipCode = a.ZipCode
                                          }).ToArray();

                return new ObjectResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.InternalError, ex, "AddressController GetAllAddresses caused an internal error.");
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
            finally
            {
                stopwatch.Stop();
                LogTimeToAppInsights(stopwatch);
            }
        }

        /// <summary>
        /// Gets all of the customer's addresses.
        /// </summary>
        /// <param name="id">The customer identifier.</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(AddressResult[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(void), 500)]
        [HttpGet]
        [Route("api/v1/customers/{id}/addresses", Name = GetAddressByIdRoute)]
        public IActionResult GetCustomerAddress(Guid id)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                // Doing this extra lookup so we can tell caller that customer id was not found
                bool customerExists = (from c in _context.Customers where c.Id == id select c).Any();

                stopwatch.Stop();
                LogTimeToAppInsights(stopwatch, CustomerLookupByIdCustomTelemetryKeyName);

                if (!customerExists)
                {
                    _logger.LogInformation(LoggingEvents.GetItem, "AddressController GetCustomerAddresss Customer id:[{id}] was not found.", id);
                    return NotFound();
                }

                stopwatch.Restart();

                AddressResult[] result = (from a in _context.Addresses
                                          where a.CustomerId == id
                                          select new AddressResult()
                                          {
                                              Id = a.AddressId ?? -1,
                                              Line1 = a.Line1,
                                              Line2 = a.Line2,
                                              City = a.City,
                                              State = a.State,
                                              ZipCode = a.ZipCode
                                          }).ToArray();

                return new ObjectResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.InternalError, ex, "AddressController GetCustomerAddresss id:[{id}] caused an internal error.", id);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
            finally
            {
                stopwatch.Stop();
                LogTimeToAppInsights(stopwatch);
            }
        }

        /// <summary>
        /// Creates the address
        /// </summary>
        /// <param name="id">The identifier of the customer the address is associated with.</param>
        /// <param name="addressCreatePayload">The address.</param>
        /// <returns>
        /// An IAction result indicating HTTP 201 created if success otherwise BadRequest if the input is not valid.
        /// </returns>
        [ProducesResponseType(typeof(AddressResult), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(void), 500)]
        [Route("api/v1/customers/{id}/addresses")]
        [HttpPost]
        public IActionResult CreateAddress(Guid id, [FromBody] AddressCreatePayload addressCreatePayload)
        {
            Address addressEntity = new Address();

            Stopwatch stopwatch = Stopwatch.StartNew();

            // Doing this extra lookup so we can tell caller that customer id was not found
            bool customerExists = (from c in _context.Customers where c.Id == id select c).Any();

            stopwatch.Stop();
            LogTimeToAppInsights(stopwatch, CustomerLookupByIdCustomTelemetryKeyName);

            if (!customerExists)
            {
                _logger.LogInformation(LoggingEvents.GetItem, "AddressController CreateAddress Customer id:[{id}] was not found.", id);
                return NotFound();
            }

            stopwatch.Restart();

            try
            {
                if (ModelState.IsValid)
                {
                    addressEntity.Line1 = addressCreatePayload.Line1;
                    addressEntity.Line2 = addressCreatePayload.Line2;
                    addressEntity.City = addressCreatePayload.City;
                    addressEntity.State = addressCreatePayload.State;
                    addressEntity.ZipCode = addressCreatePayload.ZipCode;

                    // Associate the address with the customer
                    addressEntity.CustomerId = id;

                    // Tell entity framework to add the address entity
                    _context.Addresses.Add(addressEntity);

                    _context.SaveChanges();
                }
                else
                {
                    return BadRequest(ModelState);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.InternalError, ex, "AddressController Post AddressEntity([{addressEntity}]) AddressCreatePayload([{addressCreatePayload}] caused an internal error.", addressEntity, addressCreatePayload);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
            finally
            {
                stopwatch.Stop();
                LogTimeToAppInsights(stopwatch);
            }
            AddressResult result = new AddressResult()
            {
                Id = addressEntity.AddressId ?? -1,
                City = addressEntity.City,
                State = addressEntity.State,
                Line1 = addressEntity.Line1,
                Line2 = addressEntity.Line2,
                ZipCode = addressEntity.ZipCode
            };

            return CreatedAtRoute(GetAddressByIdRoute, new { id = addressEntity.CustomerId, addressId = addressEntity.AddressId }, result);
        }

        /// <summary>
        /// Gets the customer's address.
        /// </summary>
        /// <param name="id">The customer identifier.</param>
        /// <param name="addressId">The address identifier.</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(AddressResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(void), 500)]
        [HttpGet]
        [Route("api/v1/customers/{id}/addresses/{addressId}")]
        public IActionResult GetCustomerAddressById(Guid id, int addressId)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {

                // Doing this extra lookup so that we can tell caller about an invalid customer id.
                bool customerExists = (from c in _context.Customers where c.Id == id select c).Any();

                stopwatch.Stop();
                LogTimeToAppInsights(stopwatch, CustomerLookupByIdCustomTelemetryKeyName);

                if (!customerExists)
                {
                    _logger.LogInformation(LoggingEvents.GetItem, "AddressController GetAddress Customer id:[{id}] was not found.", id);
                    return NotFound();
                }

                stopwatch.Restart();
                AddressResult? address = (from a in _context.Addresses
                                          where a.AddressId == addressId && a.CustomerId == id
                                          select new AddressResult()
                                          {
                                              Id = a.AddressId ?? -1,
                                              Line1 = a.Line1,
                                              Line2 = a.Line2,
                                              City = a.City,
                                              State = a.State,
                                              ZipCode = a.ZipCode
                                          }).SingleOrDefault();

                if (address == null)
                {
                    _logger.LogInformation(LoggingEvents.GetItem, "AddressController GetAddress addressId=[{addressId}] was not found.", addressId);
                    return NotFound();
                }

                return new ObjectResult(address);
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.InternalError, ex, "AddressController GetAddress id:[{id}], address=[{addressId}]) caused an internal error.", id, addressId);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
            finally
            {
                stopwatch.Stop();
                LogTimeToAppInsights(stopwatch);
            }
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
