using Microsoft.AspNetCore.Mvc;
using AppInsightsDemo.PublicModels;
using System.Text.Json;
using Microsoft.ApplicationInsights.Metrics;

namespace AppInsightsDemo.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public partial class CustomersController : ControllerBase
    {
        /// <summary>
        /// Generates a fake cost using Random
        /// </summary>
        /// <returns>A fake cost</returns>
        private double GetFakeCost()
        {
            Random random = new Random(DateTime.UtcNow.Millisecond);
            double fakeCost = random.NextDouble() * 100;
            return fakeCost; 
        }

        /// <summary>
        /// Return a list of questions
        /// </summary>
        /// <returns>The set of questions for the user to answer</returns>
        [HttpGet(Name = GetCustomersRouteName)]
        public ActionResult<IEnumerable<Customer>> GetAll()
        {
            // DEMO: Log an event to application insights
            //       Indicating the retrieve all was called
            //       With a fake cost
            double fakeCost = GetFakeCost();
            _logger.LogInformation("GetAll: Fake Cost {fakeCost}", fakeCost);

            _telemetryClient.TrackEvent("AllCustomersRetrieved",
                            properties: new Dictionary<string, string>()
                            {
                                            { "Count", _customers.Count.ToString() }
                            },
                            metrics: new Dictionary<string, double>()
                            {
                                            { "Cost", fakeCost }
                            });

            return _customers.Values;
        }

        /// <summary>
        /// Return the specific customer by id
        /// </summary>
        /// <returns>The set of questions for the user to answer</returns>
        /// <param name="id">The id of the customer to be returned</param>
        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public ActionResult<Customer> GetById(int id)
        {

            // Specifically not checking to see if the id exists to
            // demonstrate the snapshot debugger. 

            // DEMO: Log an event to application insights
            //       Indicating the retrieve all was called
            //       With a fake cost
            double fakeCost = GetFakeCost();
            _logger.LogInformation("GetById [{id}]: Fake Cost {fakeCost}", id,fakeCost);

            _telemetryClient.TrackEvent("ACustomerRetrieved",
                            properties: new Dictionary<string, string>()
                            {
                                            { "Count", "1" },
                                            {"CustomerName",_customers[id].Name }
                            },
                            metrics: new Dictionary<string, double>()
                            {
                                            { "Cost", fakeCost }
                            });
            return _customers[id];
        }

        /// <summary>
        /// Returning the count of customers
        /// </summary>
        /// <remarks>Head requests typically return metadata about the resource</remarks>
        [HttpHead]
        public ActionResult GetCustomerCount()
        {
            Response.Headers.Add("X-Total-Count", _customers?.Count.ToString());
            return Ok();
        }

        /// <summary>
        /// Creates the customers specified in the batchOfCustomersToCreate
        /// </summary>
        /// <param name="batchOfCustomersToCreate">The batch of customers to create</param>
        /// <remarks>A customer name of "Bad Robot" will generate an internal server error</remarks>
        [HttpPost]
        [ProducesResponseType(typeof(List<Customer>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public ActionResult CreateCustomers([FromBody] BatchOfCustomersToCreate batchOfCustomersToCreate)
        {
            // DEMO: Track a metric indicating how many customers in the batch
            MetricIdentifier metricIdentifier = new MetricIdentifier(metricNamespace: "BatchOperations",
                                                                     metricId: "CustomersUploaded");
            _telemetryClient.GetMetric(metricIdentifier).TrackValue(batchOfCustomersToCreate?.Customers?.Count);

            // DEMO: Log a custom property that contains the batchOfCustomersToCreate to app insights
            IDictionary<string, string> properties = new Dictionary<string, string>();
            properties.Add("BatchOfCustomersToCreate", JsonSerializer.Serialize(batchOfCustomersToCreate));
            _telemetryClient.TrackTrace("CreateCustomers", properties);

            try
            {

                List<Customer> customersAdded;
                if (batchOfCustomersToCreate?.Customers?.Count > 0)
                {
                    // Ensure no more than MaxBatchCustomers at a time
                    if (batchOfCustomersToCreate.Customers.Count > _customerLimits.MaxBatchCustomers)
                    {
                        return BadRequest(new ErrorResponse()
                        {
                            ErrorNumber = ErrorResponse.ToManyBatchCustomers,
                            Description = $"There are to many customers in your batch, we only support {_customerLimits.MaxBatchCustomers} at a time"
                        });
                    }

                    // Ensure no more than MaxCustomers in total
                    if ((batchOfCustomersToCreate.Customers.Count + _customers.Count) > _customerLimits.MaxCustomers)
                    {
                        return BadRequest(new ErrorResponse()
                        {
                            ErrorNumber = ErrorResponse.ToManyCustomers,
                            Description = $"Adding the batch of customers would result in more than {_customerLimits.MaxCustomers}. There are currently {_customers.Count} in the system."
                        });
                    }

                    customersAdded = new List<Customer>();

                    foreach (CustomerCreatePayload customer in batchOfCustomersToCreate.Customers)
                    {
                        if (string.IsNullOrWhiteSpace(customer.Name))
                        {
                            return BadRequest("Customers must include their name");
                        }

                        // Not thread safe
                        int nextId = _customers.Keys.Max() + 1;

                        // Simulate an internal error
                        if (customer.Name == "Bad Robot")
                        {
                            throw new Exception("Bad Robot Error!");
                        }

                        if (customer.Name == "System Exception")
                        {
                            throw new SystemException("Testing snapshot debugger");
                        }

                        Customer customerToAdd = customer.ToCustomer(nextId);
                        _customers.Add(nextId, customerToAdd);
                        customersAdded.Add(customerToAdd);

                        // DEMO: Log an event to application insights
                        //       Indicating the customer added
                        //       A fake cost
                        double fakeCost = GetFakeCost();
                        _logger.LogInformation("CreateCustomers[{nextId}]: Fake Cost {fakeCost}", nextId,fakeCost);

                        _telemetryClient.TrackEvent("CustomerAdded",
                                        properties: new Dictionary<string, string>()
                                        {
                                            { "CustomerName", customerToAdd.Name }
                                        },
                                        metrics: new Dictionary<string, double>()
                                        {
                                            { "Cost", fakeCost }
                                        });
                    }
                }
                else
                {
                    return BadRequest(new ErrorResponse()
                    {
                        ErrorNumber = ErrorResponse.NoCustomersProvided,
                        Description = "There are no customers provided"
                    });
                }

                return CreatedAtRoute(GetCustomersRouteName, customersAdded);
            }
            catch (SystemException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // DEMO: Log a exception with custom property that contains the batchOfCustomersToCreate 
                // to app insights
                _telemetryClient.TrackException(ex, properties);

                return this.StatusCode(StatusCodes.Status500InternalServerError,
                    new ErrorResponse()
                    {
                        ErrorNumber = ErrorResponse.InternalError,
                        Description = "An internal error occurred"
                    });
            }
        }
    }
}
