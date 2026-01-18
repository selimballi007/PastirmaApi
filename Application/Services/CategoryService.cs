using Microsoft.EntityFrameworkCore;
using PastirmaApi.Application.DTOs.CategoryDTOs;
using PastirmaApi.Application.Interfaces.Services;
using PastirmaApi.Core.Entities;
using PastirmaApi.Core.Exceptions;
using PastirmaApi.Infrastructure.Data;

namespace PastirmaApi.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(ApplicationDbContext context, ILogger<CategoryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<CategoryDTO> CreateCategoryAsync(CreateCategoryDTO dto)
        {
            try
            {
                // ? Ayný isimde kategori var mý?
                var exists = await _context.Categories
                    .AnyAsync(c => c.Name.ToLower() == dto.Name.ToLower());

                if (exists)
                {
                    throw new BusinessException("Bu isimde bir kategori zaten mevcut.");
                }

                // ? En yüksek DisplayOrder'ý bul
                var maxOrder = await _context.Categories
                    .MaxAsync(c => (int?)c.DisplayOrder) ?? 0;

                var category = new Category
                {
                    Name = dto.Name,
                    Icon = dto.Icon ?? "??",
                    DisplayOrder = maxOrder + 1
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                return MapToDto(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category: {Name}", dto.Name);
                throw;
            }
        }

        public async Task<CategoryDTO> UpdateCategoryAsync(int id, UpdateCategoryDTO dto)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    throw new BusinessException("Kategori bulunamadý.");
                }

                // ? Farklý bir kategori ayný isme sahip mi?
                var exists = await _context.Categories
                    .AnyAsync(c => c.Name.ToLower() == dto.Name.ToLower() && c.Id != id);

                if (exists)
                {
                    throw new BusinessException("Bu isimde bir kategori zaten mevcut.");
                }

                category.Name = dto.Name;
                category.Icon = dto.Icon ?? category.Icon;
                category.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return MapToDto(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category: {CategoryId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                {
                    throw new BusinessException("Kategori bulunamadý.");
                }

                // ? Kategoride ürün var mý kontrol et
                if (category.Products.Any())
                {
                    throw new BusinessException(
                        $"Bu kategoride {category.Products.Count} adet ürün var. Önce ürünleri baþka kategoriye taþýyýn veya silin."
                    );
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category: {CategoryId}", id);
                throw;
            }
        }

        public async Task<CategoryDTO> GetCategoryByIdAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                throw new BusinessException("Kategori bulunamadý.");
            }

            return MapToDto(category);
        }

        public async Task<List<CategoryDTO>> GetAllCategoriesAsync(bool includeInactive = false)
        {
            var query = _context.Categories.AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(c => c.IsActive);
            }

            var categories = await query
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            return categories.Select(MapToDto).ToList();
        }

        public async Task<List<CategoryWithProductCountDTO>> GetCategoriesWithProductCountAsync()
        {
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .Select(c => new CategoryWithProductCountDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    Icon = c.Icon,
                    DisplayOrder = c.DisplayOrder,
                    IsActive = c.IsActive,
                    ProductCount = c.Products.Count(p => p.IsActive), // Sadece aktif ürünler
                    CreatedAt = c.CreatedDate
                })
                .ToListAsync();

            return categories;
        }

        public async Task<bool> ReorderCategoriesAsync(List<ReorderCategoryDTO> categories)
        {
            try
            {
                foreach (var item in categories)
                {
                    var category = await _context.Categories.FindAsync(item.Id);
                    if (category != null)                    
                        category.DisplayOrder = item.DisplayOrder;
                    
                }

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering categories");
                throw;
            }
        }

        public async Task<bool> ToggleCategoryStatusAsync(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    throw new BusinessException("Kategori bulunamadý.");
                }

                category.IsActive = !category.IsActive;
                category.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return category.IsActive;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling category status: {CategoryId}", id);
                throw;
            }
        }

        // Private helper method
        private CategoryDTO MapToDto(Category category)
        {
            return new CategoryDTO
            {
                Id = category.Id,
                Name = category.Name,
                Icon = category.Icon,
                DisplayOrder = category.DisplayOrder,
                IsActive= category.IsActive
            };
        }
    }
}
