using System.Net;
using Microsoft.AspNetCore.Mvc;
using Azure.Data.Tables;
using AzureTablesDemo2.Models;
using Azure;

namespace AzureTables2.Controllers
{
    /// <summary>
    /// Implements the resource verbs for the internallogentry resource
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class InternalLogEntryController : ControllerBase
    {

        /// <summary>
        /// The get by identifier route name
        /// </summary>
        private const string GetInternalLogEntryByIdRouteName = "GetInternalLogEntryByIdRouteName";

        /// <summary>
        /// The table client used for accessing the table
        /// </summary>
        private readonly TableClient _tableClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="InternalLogEntryController"/> class.
        /// </summary>
        public InternalLogEntryController(TableServiceClient tableServiceClient)
        {
            _tableClient = tableServiceClient.GetTableClient(TableConstants.LOG_TABLENAME);            
        }
        /// <summary>
        /// Retrieves the the external log entry by its id
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>InternalLogEntry.</returns>
        [ProducesResponseType(typeof(InternalLogEntry), StatusCodes.Status200OK)]
        [Route("/api/v1/internallogentry/{id}", Name = GetInternalLogEntryByIdRouteName)]
        [HttpGet]
        public async Task<IActionResult> Get(Guid id)
        {

            try
            {
                // Create the table if it does not exist
                await _tableClient.CreateIfNotExistsAsync();

                // Retrieve a single log entry
                var queryResult = _tableClient.GetEntity<InternalLogEntry>(partitionKey: TableConstants.INTERNALLOG_PARTITIONKEY,
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
        /// Adds the specified internal log entry.
        /// </summary>
        /// <param name="logEntryInput">The log entry.</param>
        /// <returns>An ExternalLogEntry that includes information about the row creation including the key, etag and partition</returns>
        [ProducesResponseType(typeof(RowResult), (int)HttpStatusCode.Created)]
        [Route("/api/v1/internallogentry")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] InternalLogEntryInput logEntryInput)
        {
            try
            {
                // Create the table if it does not exist
                await _tableClient.CreateIfNotExistsAsync();

                InternalLogEntry logEntry = new InternalLogEntry(logEntryInput);

                // Assign a unique row key
                logEntry.RowKey = Guid.NewGuid().ToString();

                // Assign the partition key
                logEntry.PartitionKey = TableConstants.INTERNALLOG_PARTITIONKEY;

                var result = await _tableClient.AddEntityAsync(logEntry);

                if (!result.IsError)
                {
                    return CreatedAtRoute(GetInternalLogEntryByIdRouteName, new { id = logEntry.RowKey }, new RowResult { RowKey = logEntry.RowKey, PartitionKey = logEntry.PartitionKey, Etag = logEntry.ETag.ToString() });
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
