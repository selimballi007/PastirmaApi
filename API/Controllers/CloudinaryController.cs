using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PastirmaApi.Application.DTOs.CloudinaryDTOs;
using PastirmaApi.Application.Interfaces.Services;

namespace PastirmaApi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] // Only admins can manage Cloudinary images
    public class CloudinaryController : ControllerBase
    {
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ILogger<CloudinaryController> _logger;

        public CloudinaryController(
            ICloudinaryService cloudinaryService,
            ILogger<CloudinaryController> logger)
        {
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }

        /// <summary>
        /// Get all images from Cloudinary with database usage tracking
        /// GET /api/cloudinary/images
        /// </summary>
        [HttpGet("images")]
        public async Task<ActionResult<List<CloudinaryImageDTO>>> GetAllImages()
        {
            try
            {
                var images = await _cloudinaryService.GetAllImagesAsync();
                return Ok(images);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Cloudinary images");
                return StatusCode(500, new { message = "Error fetching images from Cloudinary", error = ex.Message });
            }
        }

        /// <summary>
        /// Get products using a specific image URL
        /// GET /api/cloudinary/images/usage?imageUrl={url}
        /// </summary>
        [HttpGet("images/usage")]
        public async Task<ActionResult<List<ProductUsageDTO>>> GetImageUsage([FromQuery] string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                {
                    return BadRequest(new { message = "Image URL is required" });
                }

                var usage = await _cloudinaryService.GetProductsUsingImageAsync(imageUrl);
                return Ok(usage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking image usage");
                return StatusCode(500, new { message = "Error checking image usage", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete an image from Cloudinary and optionally update database references
        /// DELETE /api/cloudinary/images/{publicId}?updateDatabase=true
        /// </summary>
        [HttpDelete("images/{publicId}")]
        public async Task<ActionResult<CloudinaryDeleteResultDTO>> DeleteImage(
            string publicId,
            [FromQuery] bool updateDatabase = true)
        {
            try
            {
                if (string.IsNullOrEmpty(publicId))
                {
                    return BadRequest(new { message = "Public ID is required" });
                }

                var result = await _cloudinaryService.DeleteImageAsync(publicId, updateDatabase);

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image: {PublicId}", publicId);
                return StatusCode(500, new { message = "Error deleting image", error = ex.Message });
            }
        }

        /// <summary>
        /// Update product image URLs when Cloudinary URL changes
        /// PUT /api/cloudinary/images/update-urls
        /// </summary>
        [HttpPut("images/update-urls")]
        public async Task<ActionResult> UpdateImageUrls([FromBody] UpdateImageUrlsRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.OldUrl) || string.IsNullOrEmpty(request.NewUrl))
                {
                    return BadRequest(new { message = "Both old and new URLs are required" });
                }

                // Only update if URLs are different
                if (request.OldUrl == request.NewUrl)
                {
                    return Ok(new { message = "URLs are the same, no update needed" });
                }

                var success = await _cloudinaryService.UpdateProductImageUrlsAsync(request.OldUrl, request.NewUrl);

                if (success)
                {
                    return Ok(new { message = "Product image URLs updated successfully" });
                }
                else
                {
                    return StatusCode(500, new { message = "Failed to update product image URLs" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating image URLs");
                return StatusCode(500, new { message = "Error updating image URLs", error = ex.Message });
            }
        }
    }

    public class UpdateImageUrlsRequest
    {
        public string OldUrl { get; set; } = string.Empty;
        public string NewUrl { get; set; } = string.Empty;
    }
}
