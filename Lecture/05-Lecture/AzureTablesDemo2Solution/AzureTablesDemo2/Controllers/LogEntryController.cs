using System.Net;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using Azure.Data.Tables;

namespace AzureTables2.Controllers
{
    /// <summary>
    /// /// Implements the resource verbs for the logentry resource
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class LogEntryController : ControllerBase
    {
        /// <summary>
        /// The configuration instance
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// The table service client instance
        /// </summary>
        private readonly TableServiceClient _tableServiceClient;

        /// <summary>
        /// Logger instance
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogEntryController"/> class.
        /// </summary>
        public LogEntryController(ILogger<LogEntryController> logger,
                                  IConfiguration configuration,
                                  TableServiceClient tableServiceClient )
        {
            _logger = logger;
            _configuration = configuration;
            _tableServiceClient = tableServiceClient;
        }

        /// <summary>
        /// Retrieves the all of the log entries both internal and external
        /// </summary>
        /// <returns>ArrayList.</returns>
        [ProducesResponseType(typeof(ArrayList), (int)HttpStatusCode.OK)]
        [Route("/api/v1/logentry")]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            // Get a table service client to access the table
            TableClient? tableClient = _tableServiceClient.GetTableClient(TableConstants.LOG_TABLENAME);

            // Create the table if it does not exist
            await tableClient.CreateIfNotExistsAsync();

            Azure.Pageable<TableEntity>? queryResults = tableClient.Query<TableEntity>();

            return new ObjectResult(queryResults);

        }

    }
}
