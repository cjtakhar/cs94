using System.Net;
using Microsoft.AspNetCore.Mvc;
using Azure.Data.Tables;
using AzureTablesDemo2.Models;
using Azure;

namespace AzureTables2.Controllers
{
    /// <summary>
    /// Implements the resource verbs for the externallogentry resource
    /// </summary>    
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ExternalLogEntryController : ControllerBase
    {
        /// <summary>
        /// The configuration instance
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Logger instance
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// The get by identifier route name
        /// </summary>
        private const string GetExternalLogEntryByIdRouteName = "GetExternalLogEntryByIdRouteName";

        /// <summary>
        /// The table client used for accessing the table
        /// </summary>
        private readonly TableClient _tableClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalLogEntryController"/> class.
        /// </summary>
        public ExternalLogEntryController(ILogger<ExternalLogEntryController> logger,
                                          IConfiguration configuration,
                                          TableServiceClient tableServiceClient)
        {
            _logger = logger;
            _configuration = configuration;
            _tableClient = tableServiceClient.GetTableClient(TableConstants.LOG_TABLENAME);
        }

        /// <summary>
        /// Retrieves the the external log entry by its id
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>ExternalLogEntry.</returns>        
        [ProducesResponseType(typeof(ExternalLogEntry), StatusCodes.Status200OK)]
        [Route("/api/v1/externallogentry/{id}", Name = GetExternalLogEntryByIdRouteName)]
        [HttpGet]
        public async Task<IActionResult> Get(Guid id)
        {

            try
            {
                // Create the table if it does not exist
                await _tableClient.CreateIfNotExistsAsync();

                // Retrieve a single log entry
                var queryResult = _tableClient.GetEntity<ExternalLogEntry>(partitionKey: TableConstants.EXTERNALLOG_PARTITIONKEY,
                                                                     rowKey: id.ToString());

                if (queryResult.GetRawResponse().Status == StatusCodes.Status200OK)
                {
                    return new ObjectResult(queryResult.Value);
                }

                return NotFound();
            }
            catch (RequestFailedException ex)
            {
                return StatusCode(ex.Status);
            }
        }

        /// <summary>
        /// Adds the specified external entry.
        /// </summary>
        /// <param name="logEntryInput">The log entry.</param>
        /// <returns>An ExternalLogEntry that includes information about the row creation including the key, etag and partition</returns>
        [ProducesResponseType(typeof(RowResult), (int)HttpStatusCode.Created)]
        [Route("/api/v1/externallogentry")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ExternalLogEntryInput logEntryInput)
        {

            try
            {
                // Create the table if it does not exist
                await _tableClient.CreateIfNotExistsAsync();

                ExternalLogEntry logEntry = new ExternalLogEntry(logEntryInput);

                // Assign a unique row key
                logEntry.RowKey = Guid.NewGuid().ToString();

                // Assign the partition key
                logEntry.PartitionKey = TableConstants.EXTERNALLOG_PARTITIONKEY;

                var result = await _tableClient.AddEntityAsync(logEntry);

                if (!result.IsError)
                {
                    return CreatedAtRoute(GetExternalLogEntryByIdRouteName, new { id = logEntry.RowKey }, new RowResult { RowKey = logEntry.RowKey, PartitionKey = logEntry.PartitionKey, Etag = logEntry.ETag.ToString() });
                }
                else
                {
                    return StatusCode(result.Status, "Failed to create the row");
                }
            }
            catch (RequestFailedException ex)
            {
                return StatusCode(ex.Status);
            }
        }
    }
}
