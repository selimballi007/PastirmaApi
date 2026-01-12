using PastirmaApi.Application.DTOs.CloudinaryDTOs;

namespace PastirmaApi.Application.Interfaces.Services
{
    public interface ICloudinaryService
    {
        /// <summary>
        /// Get all images from Cloudinary with database usage tracking
        /// </summary>
        Task<List<CloudinaryImageDTO>> GetAllImagesAsync();

        /// <summary>
        /// Delete an image from Cloudinary and update database references
        /// </summary>
        /// <param name="publicId">Cloudinary public ID</param>
        /// <param name="updateDatabase">If true, removes image URLs from products using it</param>
        Task<CloudinaryDeleteResultDTO> DeleteImageAsync(string publicId, bool updateDatabase = true);

        /// <summary>
        /// Get products using a specific Cloudinary URL
        /// </summary>
        Task<List<ProductUsageDTO>> GetProductsUsingImageAsync(string imageUrl);

        /// <summary>
        /// Update product image URLs when Cloudinary image is replaced
        /// </summary>
        Task<bool> UpdateProductImageUrlsAsync(string oldUrl, string newUrl);
    }
}
