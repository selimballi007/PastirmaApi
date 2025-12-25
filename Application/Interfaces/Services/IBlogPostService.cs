using PastirmaApi.Application.DTOs.BlogPostDTOs;

namespace PastirmaApi.Application.Interfaces.Services
{
    public interface IBlogPostService
    {
        Task<BlogPostDTO> CreatePostAsync(CreateBlogPostDTO dto, int userId);
        Task<BlogPostDTO> UpdatePostAsync(int id, UpdateBlogPostDTO dto);
        Task<bool> DeletePostAsync(int id);
        Task<BlogPostDTO> GetPostByIdAsync(int id, bool incrementViewCount = false);
        Task<List<BlogPostListDTO>> GetAllPostsAsync(bool includeInactive = false);
        Task<List<BlogPostListDTO>> GetPublishedPostsAsync();
        Task<List<BlogPostListDTO>> GetFeaturedPostsAsync();
        Task<bool> TogglePostStatusAsync(int id);
        Task<bool> ToggleFeaturedAsync(int id);
    }
}
