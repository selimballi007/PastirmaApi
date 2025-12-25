using PastirmaApi.Application.DTOs.BlogCategoryDTOs;

namespace PastirmaApi.Application.Interfaces.Services
{
    public interface IBlogCategoryService
    {
        Task<BlogCategoryDTO> CreateCategoryAsync(CreateBlogCategoryDTO dto);
        Task<BlogCategoryDTO> UpdateCategoryAsync(int id, CreateBlogCategoryDTO dto);
        Task<bool> DeleteCategoryAsync(int id);
        Task<BlogCategoryDTO> GetCategoryByIdAsync(int id);
        Task<List<BlogCategoryDTO>> GetAllCategoriesAsync(bool includeInactive = false);
        Task<List<BlogCategoryDTO>> GetActiveCategoriesAsync();
    }
}
