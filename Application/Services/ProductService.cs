using Microsoft.EntityFrameworkCore;
using PastirmaApi.Application.DTOs.ProductDTOs;
using PastirmaApi.Application.Interfaces.Services;
using PastirmaApi.Core.Entities;
using PastirmaApi.Infrastructure.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PastirmaApi.Application.Services
{
    public class ProductService: IProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProductDTO>> GetProductsAsync(ProductFiltersDTO filters)
        {
            var query = _context.Products.AsQueryable();

            // Filtreler
            if (filters.CategoryId > 0)
            {
                query = query.Where(p => p.CategoryId == filters.CategoryId);
            }

            if (filters.IsActive.HasValue)
            {
                query = query.Where(p => p.IsActive == filters.IsActive.Value);
            }

            if (filters.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= filters.MinPrice.Value);
            }

            if (filters.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= filters.MaxPrice.Value);
            }

            if (!string.IsNullOrWhiteSpace(filters.Search))
            {
                var searchTerm = filters.Search.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchTerm) ||
                    (p.Description != null && p.Description.ToLower().Contains(searchTerm)));
            }

            if (filters.IsBestSeller)
            {
                query = query.Where(p => p.IsBestseller);
            }

            if (filters.IsCampaign)
            {
                query = query.Where(p => p.IsCampaign);
            }

            if (filters.Limit.HasValue && filters.Limit.Value > 0)
            {
                query = query.Take(filters.Limit.Value);
            }

            var products = await query
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();

            return products.Select(MapToDto).ToList();
        }
        public async Task<ProductDTO?> GetProductByIdAsync(int id, bool includeImages=true)
        {
            var query = _context.Products
                .Where(p => p.Id == id && p.IsActive);                

            if (includeImages)            
                query = query.Include(p => p.Images);            

            var product = await query
                .Include(p=>p.Category)
                .FirstOrDefaultAsync();
            return product != null ? MapToDto(product) : null;
        }
        public async Task<ProductDTO> CreateProductAsync(CreateProductRequestDTO request)
        {
            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Stock = request.Stock,
                CategoryId = request.CategoryId,
                ImageUrl = request.ImageUrl ?? string.Empty ,
                IsActive = request.IsActive
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return MapToDto(product);
        }
        public async Task<ProductDTO?> UpdateProductAsync(int id, UpdateProductRequestDTO request)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return null;
            }

            product.Name = request.Name;
            product.Description = request.Description;
            product.Price = request.Price;
            product.Stock = request.Stock;
            product.CategoryId = request.CategoryId;
            product.ImageUrl = request.ImageUrl ?? string.Empty;
            product.IsActive = request.IsActive;

            // Handle images update
            if (request.Images != null)
            {
                // Remove existing images
                _context.ProductImages.RemoveRange(product.Images);

                // Add new images
                foreach (var imageDto in request.Images)
                {
                    product.Images.Add(new ProductImage
                    {
                        ImageUrl = imageDto.ImageUrl,
                        DisplayOrder = imageDto.DisplayOrder,
                        IsPrimary = imageDto.IsPrimary,
                        ProductId = product.Id
                    });
                }
            }

            await _context.SaveChangesAsync();

            return MapToDto(product);
        }
        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return false;
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<bool> UpdateProductStatusAsync(int id, bool isActive)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return false;
            }

            product.IsActive = isActive;

            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<bool> UpdateProductStockAsync(int id, int stock)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return false;
            }

            product.Stock = stock;

            await _context.SaveChangesAsync();

            return true;
        }

        // Helper method
        private ProductDTO MapToDto(Product product)
        {
            return new ProductDTO
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                CategoryId = product.CategoryId,
                ImageUrl = product.ImageUrl,
                IsActive = product.IsActive,
                CreatedAt= product.CreatedDate,
                UpdatedAt = product.UpdatedDate ?? DateTime.MinValue ,
                OldPrice = product.OldPrice,
                CategoryName= product.Category.Name,
                IsCampaign= product.IsCampaign,
                IsBestSeller = product.IsBestseller,
                IsNew = product.IsNew,
                IsSpecialOffer= product.IsSpecialOffer,
                CampaignOrder = product.CampaignOrder,
                BestSellerOrder = product.BestsellerOrder,
                Images = product.Images?.Select(img => new ProductImageDTO
                {
                    ImageUrl = img.ImageUrl,
                    DisplayOrder = img.DisplayOrder,
                    IsPrimary = img.IsPrimary
                }).ToList(),
                Rating = 0,
                ReviewCount= 0,
                SalesCount = 0
            };
        }
    }
}
