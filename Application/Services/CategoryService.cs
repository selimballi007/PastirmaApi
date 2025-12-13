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
                // ✅ Aynı isimde kategori var mı?
                var exists = await _context.Categories
                    .AnyAsync(c => c.Name.ToLower() == dto.Name.ToLower());

                if (exists)
                {
                    throw new InvalidOperationException("Bu isimde bir kategori zaten mevcut.");
                }

                // ✅ En yüksek DisplayOrder'ı bul
                var maxOrder = await _context.Categories
                    .MaxAsync(c => (int?)c.DisplayOrder) ?? 0;

                var category = new Category
                {
                    Name = dto.Name,
                    Icon = dto.Icon ?? "📦",
                    DisplayOrder = maxOrder + 1
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Category created: {CategoryId}, Name: {Name}",
                    category.Id, category.Name);

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
                    throw new NotFoundException("Kategori bulunamadı.");
                }

                // ✅ Farklı bir kategori aynı isme sahip mi?
                var exists = await _context.Categories
                    .AnyAsync(c => c.Name.ToLower() == dto.Name.ToLower() && c.Id != id);

                if (exists)
                {
                    throw new InvalidOperationException("Bu isimde bir kategori zaten mevcut.");
                }

                category.Name = dto.Name;
                category.Icon = dto.Icon ?? category.Icon;
                category.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Category updated: {CategoryId}, Name: {Name}", id, dto.Name);

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
                    throw new NotFoundException("Kategori bulunamadı.");
                }

                // ✅ Kategoride ürün var mı kontrol et
                if (category.Products.Any())
                {
                    throw new InvalidOperationException(
                        $"Bu kategoride {category.Products.Count} adet ürün var. Önce ürünleri başka kategoriye taşıyın veya silin."
                    );
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Category deleted: {CategoryId}, Name: {Name}",
                    id, category.Name);

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
                throw new NotFoundException("Kategori bulunamadı.");
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

                _logger.LogInformation("Categories reordered: {Count} categories", categories.Count);

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
                    throw new NotFoundException("Kategori bulunamadı.");
                }

                category.IsActive = !category.IsActive;
                category.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Category status toggled: {CategoryId}, IsActive: {IsActive}",
                    id, category.IsActive);

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
