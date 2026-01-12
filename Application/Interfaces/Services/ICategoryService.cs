using PastirmaApi.Application.DTOs.CategoryDTOs;

namespace PastirmaApi.Application.Interfaces.Services
{
    public interface ICategoryService
    {
        Task<CategoryDTO> CreateCategoryAsync(CreateCategoryDTO dto);
        Task<CategoryDTO> UpdateCategoryAsync(int id, UpdateCategoryDTO dto);
        Task<bool> DeleteCategoryAsync(int id);
        Task<CategoryDTO> GetCategoryByIdAsync(int id);
        Task<List<CategoryDTO>> GetAllCategoriesAsync(bool includeInactive = false);
        Task<List<CategoryWithProductCountDTO>> GetCategoriesWithProductCountAsync();
        Task<bool> ReorderCategoriesAsync(List<ReorderCategoryDTO> categories);
        Task<bool> ToggleCategoryStatusAsync(int id);
    }
}
