using Microsoft.EntityFrameworkCore;
using PastirmaApi.Application.DTOs.BlogCategoryDTOs;
using PastirmaApi.Application.Interfaces.Services;
using PastirmaApi.Core.Entities;
using PastirmaApi.Core.Exceptions;
using PastirmaApi.Infrastructure.Data;

namespace PastirmaApi.Application.Services
{
    public class BlogCategoryService : IBlogCategoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BlogCategoryService> _logger;

        public BlogCategoryService(ApplicationDbContext context, ILogger<BlogCategoryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<BlogCategoryDTO> CreateCategoryAsync(CreateBlogCategoryDTO dto)
        {
            try
            {
                var category = new BlogCategory
                {
                    Name = dto.Name,
                    Icon = dto.Icon,
                    DisplayOrder = dto.DisplayOrder
                };

                _context.BlogCategories.Add(category);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Blog category created: {CategoryId}, Name: {Name}", category.Id, category.Name);

                return MapToDto(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating blog category: {Name}", dto.Name);
                throw;
            }
        }

        public async Task<BlogCategoryDTO> UpdateCategoryAsync(int id, CreateBlogCategoryDTO dto)
        {
            try
            {
                var category = await _context.BlogCategories.FindAsync(id);
                if (category == null)
                {
                    throw new NotFoundException("Kategori bulunamadı.");
                }

                category.Name = dto.Name;
                category.Icon = dto.Icon;
                category.DisplayOrder = dto.DisplayOrder;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Blog category updated: {CategoryId}, Name: {Name}", id, dto.Name);

                return MapToDto(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating blog category: {CategoryId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            try
            {
                var category = await _context.BlogCategories.FindAsync(id);
                if (category == null)
                {
                    throw new NotFoundException("Kategori bulunamadı.");
                }

                // Check if category has blog posts
                var hasPosts = await _context.BlogPosts.AnyAsync(p => p.CategoryId == id);
                if (hasPosts)
                {
                    throw new BusinessException("Bu kategoriye ait blog yazıları bulunmaktadır. Önce blog yazılarını silin veya başka kategoriye taşıyın.");
                }

                _context.BlogCategories.Remove(category);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Blog category deleted: {CategoryId}, Name: {Name}", id, category.Name);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting blog category: {CategoryId}", id);
                throw;
            }
        }

        public async Task<BlogCategoryDTO> GetCategoryByIdAsync(int id)
        {
            var category = await _context.BlogCategories.FindAsync(id);

            if (category == null)
            {
                throw new NotFoundException("Kategori bulunamadı.");
            }

            return MapToDto(category);
        }

        public async Task<List<BlogCategoryDTO>> GetAllCategoriesAsync(bool includeInactive = false)
        {
            var query = _context.BlogCategories.AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(c => c.IsActive);
            }

            var categories = await query
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            return categories.Select(MapToDto).ToList();
        }

        public async Task<List<BlogCategoryDTO>> GetActiveCategoriesAsync()
        {
            var categories = await _context.BlogCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            return categories.Select(MapToDto).ToList();
        }

        // Private helper method
        private BlogCategoryDTO MapToDto(BlogCategory category)
        {
            return new BlogCategoryDTO
            {
                Id = category.Id,
                Name = category.Name,
                Icon = category.Icon,
                DisplayOrder = category.DisplayOrder,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedDate
            };
        }
    }
}
