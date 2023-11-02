using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinimalChatApplication.Domain.Dtos;
using MinimalChatApplication.Domain.Helpers;
using MinimalChatApplication.Domain.Interfaces;
using MinimalChatApplication.Domain.Models;

namespace MinimalChatApplication.API.Controllers
{
    [Authorize]
    [Route("api/gif")]
    [ApiController]
    public class GifController : ControllerBase
    {
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IGifService _gifService;
        private readonly IConfiguration _configuration;

        public GifController(IWebHostEnvironment hostEnvironment,
            IGifService gifService,
            IConfiguration configuration)
        {
            _hostEnvironment = hostEnvironment;
            _gifService = gifService;
            _configuration = configuration;
        }


        /// <summary>
        /// Retrieves all GIF data with their URLs for use in the application.
        /// </summary>
        /// <returns>
        ///   - 200 OK with the list of GIF data and their URLs if successful,
        ///   - 500 Internal Server Error if there are exceptions during retrieval.
        /// </returns>
        /// <remarks>
        /// This endpoint retrieves all GIF data along with their URLs for use in the application.
        /// It constructs the URLs by combining the base URL from configuration with the GIF names.
        /// </remarks>
        [HttpGet("all-gifs")]
        public async Task<IActionResult> GetAllGifAsync()
        {
            string baseUrl = _configuration["BaseUrl"];

            var gifDataFromDb = await _gifService.GetAllGifDataAsync();

            var gifDataDictionary = gifDataFromDb.ToDictionary(gif => gif.GifName, gif => gif.Id);

            // Create a list of GifData with full URLs
            var gifDatas = gifDataFromDb.Select(gif => new GifResponseDto
            {
                Id = gif.Id,
                GifUrl = baseUrl + gif.GifName
            }).ToList();

            return Ok(new ApiResponse<List<GifResponseDto>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = HttpStatusMessages.GifRetrieved,
                Data = gifDatas
            });
        }



        /// <summary>
        /// Uploads a GIF file to the server and saves its data in the database.
        /// </summary>
        /// <param name="gifDataDto">The GIF data to upload, including the file.</param>
        /// <returns>
        ///   - 200 OK with the URL of the uploaded GIF file if successful,
        ///   - 400 Bad Request if the provided file is invalid or missing,
        ///   - 500 Internal Server Error for exceptions during the upload process.
        /// </returns>
        /// <remarks>
        /// This endpoint allows you to upload a GIF file to the server and saves its data in the database.
        /// If successful, it returns the URL of the uploaded GIF file. If the provided file is invalid or missing,
        /// it responds with a Bad Request. In case of exceptions during the upload process, an Internal Server Error
        /// response is returned.
        /// </remarks>
        [HttpPost("upload-gif")]
        public async Task<IActionResult> UploadGifAsync([FromForm] GifDataDto gifDataDto)
        {
            try
            {
                if (gifDataDto.file.Length > 0)
                {
                    if (!Directory.Exists(_hostEnvironment.WebRootPath + "\\Upload\\"))
                    {
                        Directory.CreateDirectory(_hostEnvironment.WebRootPath + "\\Upload\\");
                    }

                    var filePath = Path.Combine(_hostEnvironment.WebRootPath, "Upload", gifDataDto.file.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await gifDataDto.file.CopyToAsync(stream);
                        stream.Flush();
                    }

                    // Save the GIF data in the database
                    await _gifService.AddGifDataAsync(gifDataDto.file.FileName);

                    return Ok(new { imageUrl = $"/Upload/{gifDataDto.file.FileName}" });
                }
                else
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = HttpStatusMessages.InvalidFile,
                        Data= null
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = $"{HttpStatusMessages.InternalServerError}  {ex.Message}",
                    Data = null
                });
            }
        }
    }
}
