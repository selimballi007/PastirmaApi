using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PastirmaApi.Application.DTOs.CloudinaryDTOs;
using PastirmaApi.Application.Interfaces.Services;
using PastirmaApi.Infrastructure.Data;

namespace PastirmaApi.Application.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CloudinaryService> _logger;

        public CloudinaryService(
            IConfiguration configuration,
            ApplicationDbContext context,
            ILogger<CloudinaryService> logger)
        {
            _context = context;
            _logger = logger;

            var cloudName = configuration["Cloudinary:CloudName"];
            var apiKey = configuration["Cloudinary:ApiKey"];
            var apiSecret = configuration["Cloudinary:ApiSecret"];

            if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
            {
                throw new InvalidOperationException("Cloudinary configuration is missing in appsettings.json");
            }

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
        }

        public async Task<List<CloudinaryImageDTO>> GetAllImagesAsync()
        {
            try
            {
                var result = new List<CloudinaryImageDTO>();
                string? nextCursor = null;

                // Fetch all images using pagination
                do
                {
                    var listParams = new ListResourcesParams
                    {
                        Type = "upload",
                        ResourceType = ResourceType.Image,
                        MaxResults = 500,
                        NextCursor = nextCursor
                    };

                    var listResult = await _cloudinary.ListResourcesAsync(listParams);

                    if (listResult.Resources == null || !listResult.Resources.Any())
                        break;

                    foreach (var resource in listResult.Resources)
                    {
                        var imageDto = new CloudinaryImageDTO
                        {
                            PublicId = resource.PublicId,
                            Url = resource.Url?.ToString() ?? string.Empty,
                            SecureUrl = resource.SecureUrl?.ToString() ?? string.Empty,
                            Format = resource.Format,
                            Width = resource.Width,
                            Height = resource.Height,
                            Bytes = resource.Bytes,
                            CreatedAt = DateTime.TryParse(resource.CreatedAt, out var createdDate) ? createdDate : DateTime.UtcNow,
                            ResourceType = resource.ResourceType
                        };

                        // Check database usage
                        var usage = await GetProductsUsingImageAsync(resource.SecureUrl?.ToString() ?? string.Empty);
                        imageDto.UsedInProducts = usage;
                        imageDto.IsUsedInDatabase = usage.Any();

                        result.Add(imageDto);
                    }

                    nextCursor = listResult.NextCursor;

                } while (!string.IsNullOrEmpty(nextCursor));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching images from Cloudinary");
                throw new Exception("Failed to fetch images from Cloudinary", ex);
            }
        }

        public async Task<CloudinaryDeleteResultDTO> DeleteImageAsync(string publicId, bool updateDatabase = true)
        {
            var resultDto = new CloudinaryDeleteResultDTO();

            try
            {
                // First, get the image to find its URL
                var getParams = new GetResourceParams(publicId)
                {
                    ResourceType = ResourceType.Image
                };

                var resource = await _cloudinary.GetResourceAsync(getParams);
                var imageUrl = resource.SecureUrl?.ToString() ?? string.Empty;

                // Check what products are using this image
                var productsUsing = await GetProductsUsingImageAsync(imageUrl);

                // Delete from Cloudinary
                var deleteParams = new DeletionParams(publicId)
                {
                    ResourceType = ResourceType.Image
                };

                var deleteResult = await _cloudinary.DestroyAsync(deleteParams);

                if (deleteResult.Result == "ok")
                {
                    resultDto.Success = true;
                    resultDto.Message = "Image deleted successfully from Cloudinary";

                    // Update database if requested
                    if (updateDatabase && productsUsing.Any())
                    {
                        foreach (var usage in productsUsing)
                        {
                            if (usage.UsageType == "MainImage")
                            {
                                var product = await _context.Products.FindAsync(usage.ProductId);
                                if (product != null)
                                {
                                    product.ImageUrl = string.Empty; // Clear the main image
                                    resultDto.UpdatedProducts.Add(new ProductUpdateDTO
                                    {
                                        ProductId = product.Id,
                                        ProductName = product.Name,
                                        UpdateType = "MainImageRemoved"
                                    });
                                }
                            }
                            else if (usage.UsageType == "GalleryImage")
                            {
                                var productImage = await _context.ProductImages
                                    .FirstOrDefaultAsync(pi => pi.ProductId == usage.ProductId && pi.ImageUrl == imageUrl);

                                if (productImage != null)
                                {
                                    _context.ProductImages.Remove(productImage);
                                    resultDto.UpdatedProducts.Add(new ProductUpdateDTO
                                    {
                                        ProductId = usage.ProductId,
                                        ProductName = usage.ProductName,
                                        UpdateType = "GalleryImageRemoved"
                                    });
                                }
                            }
                        }

                        await _context.SaveChangesAsync();
                        resultDto.Message += $" and updated {resultDto.UpdatedProducts.Count} product(s) in database";
                    }
                }
                else
                {
                    resultDto.Success = false;
                    resultDto.Message = $"Failed to delete image from Cloudinary: {deleteResult.Result}";
                }

                return resultDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image from Cloudinary: {PublicId}", publicId);
                resultDto.Success = false;
                resultDto.Message = $"Error: {ex.Message}";
                return resultDto;
            }
        }

        public async Task<List<ProductUsageDTO>> GetProductsUsingImageAsync(string imageUrl)
        {
            var result = new List<ProductUsageDTO>();

            try
            {
                // Check main product images
                var productsWithMainImage = await _context.Products
                    .Where(p => p.ImageUrl == imageUrl)
                    .Select(p => new ProductUsageDTO
                    {
                        ProductId = p.Id,
                        ProductName = p.Name,
                        UsageType = "MainImage"
                    })
                    .ToListAsync();

                result.AddRange(productsWithMainImage);

                // Check product gallery images
                var productsWithGalleryImage = await _context.ProductImages
                    .Where(pi => pi.ImageUrl == imageUrl)
                    .Include(pi => pi.Product)
                    .Select(pi => new ProductUsageDTO
                    {
                        ProductId = pi.ProductId,
                        ProductName = pi.Product.Name,
                        UsageType = "GalleryImage"
                    })
                    .ToListAsync();

                result.AddRange(productsWithGalleryImage);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking products using image: {ImageUrl}", imageUrl);
                return result;
            }
        }

        public async Task<bool> UpdateProductImageUrlsAsync(string oldUrl, string newUrl)
        {
            try
            {
                // Update main product images
                var productsWithMainImage = await _context.Products
                    .Where(p => p.ImageUrl == oldUrl)
                    .ToListAsync();

                foreach (var product in productsWithMainImage)
                {
                    product.ImageUrl = newUrl;
                }

                // Update product gallery images
                var productImages = await _context.ProductImages
                    .Where(pi => pi.ImageUrl == oldUrl)
                    .ToListAsync();

                foreach (var productImage in productImages)
                {
                    productImage.ImageUrl = newUrl;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Updated {MainCount} main images and {GalleryCount} gallery images from {OldUrl} to {NewUrl}",
                    productsWithMainImage.Count,
                    productImages.Count,
                    oldUrl,
                    newUrl);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product image URLs from {OldUrl} to {NewUrl}", oldUrl, newUrl);
                return false;
            }
        }
    }
}
