using AzureBlobManagedIdentity.Repositories;
using Microsoft.AspNetCore.Mvc;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AzureBlobManagedIdentity.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")] // See: https://en.wikipedia.org/wiki/Media_type
    [ApiController]
    public class PicturesController : ControllerBase
    {
        private const string GetPictureByIdRoute = "GetCustomerByIdRoute";
        private readonly IStorageRepository _storageRepository;
        private readonly ILogger<PicturesController> _logger;

        public PicturesController(IStorageRepository storageRepository,
                                  ILogger<PicturesController> logger)
        {
            _storageRepository = storageRepository;
            _logger = logger;
        }

        /// <summary>
        /// Returns the list of pictures available for download
        /// </summary>
        /// <returns>The list of pictures available for download</returns>
        [HttpGet]
        public async Task<IEnumerable<string>> GetAllPictures()
        {
            return await _storageRepository.GetListOfBlobs();
        }

        /// <summary>
        /// Returns the picture identified by the picture name
        /// </summary>
        /// <param name="id">The name of the picture to return</param>
        /// <returns>The picture identified by the picture name</returns>
        [HttpGet]
        [Route("{id}", Name = GetPictureByIdRoute)]
        [ProducesResponseType(typeof(Stream), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPictureById(string id)
        {
            try
            {
                (MemoryStream memoryStream, string contentType) = await _storageRepository.GetFileAsync(id);

                // Indicate that the file is an attachment and should be downloaded
                var contentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                {
                    // Set the file name
                    FileName = id
                };
                Response.Headers.Add("Content-Disposition", contentDisposition.ToString());

                // Return the file with the specific contentType and name, which is in the id parameter
                return File(memoryStream, contentType, id);
            }
            catch (Azure.RequestFailedException ex)
            {
                _logger.LogWarning(ex.Message);
                return StatusCode(ex.Status);   
            }
        }

        /// <summary>
        /// Uploads a picture
        /// </summary>
        /// <param name="formFile">The picture to upload</param>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> UploadPicture(IFormFile formFile)
        {
            using Stream fileStream = formFile.OpenReadStream();
            await _storageRepository.UploadFile(formFile.FileName, fileStream, formFile.ContentType);

            return CreatedAtRoute(GetPictureByIdRoute, new { id = formFile.FileName }, null);
        }

        /// <summary>
        /// Deletes the picture by its Id which is the picture name
        /// </summary>        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePicture(string id)
        {
            await _storageRepository.DeleteFile(id);
            return NoContent();
        }
    }
}
