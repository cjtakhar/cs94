using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;

namespace BlobTriggerDemo
{
    public class UpdateJobWithSuccessStatus
    {
        private readonly ILogger _logger;

        public UpdateJobWithSuccessStatus(ILogger<UpdateJobWithSuccessStatus> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Updates the job table with success indication for every file that was converted and stored in the
        /// convertedimages container
        /// </summary>
        /// <param name="blobClient">The block BLOB.</param>
        /// <param name="log">The log.</param>
        /// <remarks>
        /// You will need to create a local.settings.json file to run this from visual studio
        /// See ReadMe.txt
        /// </remarks>
        [Function("UpdateJobWithSuccessStatus")]
        public async Task Run([BlobTrigger("convertedimages/{name}", Connection = ConfigSettings.STORAGE_CONNECTIONSTRING_NAME)] BlobClient blobClient)
        {
            // Retrieve the job id
            Azure.Storage.Blobs.Models.BlobProperties properties = (await blobClient.GetPropertiesAsync()).Value;
            if (properties.Metadata.ContainsKey(ConfigSettings.JOBID_METADATA_NAME))
            {
                string jobId = properties.Metadata[ConfigSettings.JOBID_METADATA_NAME];
                _logger.LogInformation($"C# Blob trigger function Processed blob\n Name:{blobClient.Name} \n JobId: [{jobId}]");

                JobTable jobTable = new JobTable(_logger, ConfigSettings.IMAGEJOBS_PARTITIONKEY);
                await jobTable.UpdateJobEntityStatus(jobId, "Success!", "Everything A-OK");
            }
            else
            {
                _logger.LogError($"The blob {blobClient.Name} is missing its {ConfigSettings.JOBID_METADATA_NAME} metadata can't update the job");
            }

        }
    }
}
