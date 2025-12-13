using PastirmaApi.Application.DTOs.ReviewDTO;
using PastirmaApi.Application.DTOs.ReviewDTOs;

namespace PastirmaApi.Application.Interfaces.Services
{
    public interface IReviewService
    {
        Task<ReviewDTO> CreateReviewAsync(int userId, CreateReviewDTO dto);
        Task<ReviewDTO> GetReviewByIdAsync(int id);
        Task<PagedResult<ReviewDTO>> GetProductReviewsAsync(int productId, int page, int pageSize);
        Task<PagedResult<ReviewDTO>> GetPendingReviewsAsync(int page, int pageSize);
        Task<bool> ApproveReviewAsync(int reviewId);
        Task<bool> RejectReviewAsync(int reviewId);
        Task<bool> DeleteReviewAsync(int reviewId, int userId);
        Task<ProductReviewStats> GetProductReviewStatsAsync(int productId);
        Task<bool> CanUserReviewProductAsync(int userId, int productId);
    }
}
