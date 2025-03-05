using CustomerDemo.Common;
using CustomerDemo.DataTransferObjects;
using CustomerDemo.ExtensionMethods;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CustomerDemo.Controllers
{
    /// <summary>
    /// Provides implementation for the customers resource
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.ControllerBase" />
    [Produces("application/json")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        /// <summary>
        /// The "database" of customers
        /// </summary>
        private static Dictionary<int, Customer> _customers = new Dictionary<int, Customer>();

        /// <summary>
        /// Logger instance
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomersController"/> class.
        /// </summary>
        public CustomersController(ILogger<CustomersController> logger)
        {
            _logger = logger;
        }

        static CustomersController()
        {
            // Initialize Data
            _customers.Add(1, new Customer() { Id = 1, Name = "Sam Flynn", EmailAddress = "sam.flynn@encorp.com", Age = 22 });
            _customers.Add(2, new Customer() { Id = 2, Name = "Captain Kirk", EmailAddress = "captain.kirk@ussenterprise.com", Age = 32 });
            _customers.Add(3, new Customer() { Id = 3, Name = "Mr Spock", EmailAddress = "mr.spock@ussenterprise.com", Age = 104 });
            _customers.Add(4, new Customer() { Id = 4, Name = "Kiera Cameron", EmailAddress = "kiera.cameron@vancouverpolice.gov", Age = 30 });
        }

        /// <summary>
        /// Gets the specified customer resource based on the id parameter.
        /// </summary>
        /// <param name="id">The customer's id.</param>
        /// <returns>The Customer resource</returns>
        /// <remarks>
        /// Demo Notes:
        /// Valid IDs are 1, 2, 3 and 4
        /// IDs > 4 will cause a 404 not found
        /// An ID = less than 1 will cause a server error
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(typeof(Customer), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(void), StatusCodes.Status500InternalServerError)]
        [Route("api/v1/customers/{id}")]
        public IActionResult Get(int id)
        {

            try
            {
                if (id < 1)
                {
                    throw new Exception("Demo server error exception");
                }
                return new ObjectResult(_customers[id]);
            }
            catch (KeyNotFoundException knfEx)
            {
                _logger.LogInformation(LoggingEvents.GetItem, knfEx, "CustomerController Customer(id=[{id}]) was not found.", id);
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.InternalError, ex, "CustomerController Customer(id=[{id}]) caused an internal error.", id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Gets the list of customers
        /// </summary>
        /// <returns>The list of customers</returns>
        [HttpGet]
        [ProducesResponseType(typeof(int), 200)]
        [Route("api/v1/customers")]
        public IActionResult Get()
        {
            return new ObjectResult(_customers.Keys.ToArray());
        }

        /// <summary>
        /// Updates the customer
        /// </summary>
        /// <param name="id">The identifier of the customer.</param>
        /// <param name="customerInput">The customer.</param>
        /// <returns>An IAction result indicating HTTP 201 no content if success otherwise BadRequest if the input is not valid.</returns>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(List<ErrorResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(List<ErrorResponse>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(List<ErrorResponse>), StatusCodes.Status409Conflict)]
        [Route("api/v1/customers/{id}")]
        [HttpPut]
        public async Task<IActionResult> Put(int id, [FromBody] CustomerInput customerInput)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    _customers[id] = new Customer()
                    {
                        Id = id,
                        Age = customerInput.Age,
                        EmailAddress = customerInput.EmailAddress,
                        IsActive = customerInput.IsActive,
                        Name = customerInput.Name
                    };
                }
                else
                {
                    List<ErrorResponse> errorResponses = new List<ErrorResponse>();

                    if (customerInput == null)
                    {
                        ErrorResponse errorResponse = new ErrorResponse();
                        (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage(ErrorCode.AllInputIsNull.ToString());
                        errorResponse.parameterName = "[Unknown]";
                        errorResponse.parameterValue = "[Unknown]";
                        errorResponses.Add(errorResponse);
                        return BadRequest(errorResponses);
                    }

                    // DEMO: Enable multi-stream read
                    // The EnableMultipleStreamReadMiddleware is needed for reading from the
                    // Request Body a second time, the first time the Request.Body is read
                    // is in the middleware for deserializing the Customer Input

                    // This allows us access to the raw input
                    using StreamReader sr = new StreamReader(Request.Body);
                    Request.Body.Seek(0, SeekOrigin.Begin);
                    string inputJsonString = await sr.ReadToEndAsync();

                    using (JsonDocument jsonDocument = JsonDocument.Parse(inputJsonString))
                    {
                        // This is an approach for determining which properties have errors and knowing the
                        // property name as its the key value
                        foreach (string key in ModelState.Keys)
                        {
                            Microsoft.AspNetCore.Mvc.ModelBinding.ModelErrorCollection? errors = ModelState[key: key]?.Errors;
                            if (errors != null && ModelState.ContainsKey(key) && ModelState[key: key] != null && ModelState[key: key]?.ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid)
                            {
                                foreach (Microsoft.AspNetCore.Mvc.ModelBinding.ModelError error in errors)
                                {
                                    string? cleansedKey = key.CleanseModelStateKey();
                                    string? camelCaseKey = cleansedKey?.ToCamelCase();
                                    string? attemptedValue = null;

                                    if (!string.IsNullOrEmpty(camelCaseKey))
                                    {
                                        attemptedValue = jsonDocument.RootElement.GetProperty(camelCaseKey).ToString();
                                    }


                                    System.Diagnostics.Trace.WriteLine($"MODEL ERROR: key:{cleansedKey} attemptedValue:{attemptedValue ?? "[null]"}, errorMessage:{error.ErrorMessage}");

                                    ErrorResponse errorResponse = new ErrorResponse();
                                    (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage(error.ErrorMessage);
                                    errorResponse.parameterName = camelCaseKey ?? "[Unknown]";
                                    errorResponse.parameterValue = jsonDocument.RootElement.GetProperty(camelCaseKey ?? "[Unknown]").ToString();
                                    errorResponses.Add(errorResponse);
                                }
                            }
                        }
                    }

                    return BadRequest(errorResponses);
                }
            }
            catch (KeyNotFoundException knfEx)
            {
                _logger.LogInformation(LoggingEvents.GetItem, knfEx, "CustomerController Customer(id=[{id}]) was not found.", id);
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.InternalError, ex, "CustomerController Customer(id=[{id}]) caused an internal error.", id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return NoContent();
        }
    }
}

