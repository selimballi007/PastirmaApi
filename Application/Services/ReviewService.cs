using Microsoft.EntityFrameworkCore;
using PastirmaApi.Application.DTOs.ReviewDTO;
using PastirmaApi.Application.DTOs.ReviewDTOs;
using PastirmaApi.Application.Interfaces.Services;
using PastirmaApi.Core.Entities;
using PastirmaApi.Core.Exceptions;
using PastirmaApi.Infrastructure.Data;

namespace PastirmaApi.Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReviewService> _logger;

        public ReviewService(ApplicationDbContext context, ILogger<ReviewService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ReviewDTO> CreateReviewAsync(int userId, CreateReviewDTO dto)
        {
            try
            {
                // ✅ Kullanıcı bu ürünü satın almış mı?
                var hasPurchased = await HasUserPurchasedProductAsync(userId, dto.ProductId);
                if (!hasPurchased)
                {
                    throw new UnauthorizedAccessException("Bu ürünü satın almadan yorum yapamazsınız.");
                }

                // ✅ Daha önce yorum yapmış mı?
                var existingReview = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.UserId == userId && r.ProductId == dto.ProductId);

                if (existingReview != null)
                {
                    throw new InvalidOperationException("Bu ürün için zaten yorum yaptınız.");
                }

                // ✅ Ürün var mı?
                var product = await _context.Products.FindAsync(dto.ProductId);
                if (product == null)
                {
                    throw new NotFoundException("Ürün bulunamadı.");
                }

                var review = new Review
                {
                    UserId = userId,
                    ProductId = dto.ProductId,
                    Rating = dto.Rating,
                    Comment = dto.Comment,
                    Status = ReviewStatus.Pending
                };

                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();

                return await GetReviewByIdAsync(review.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review for UserId={UserId}, ProductId={ProductId}",
                    userId, dto.ProductId);
                throw;
            }
        }

        public async Task<ReviewDTO> GetReviewByIdAsync(int id)
        {
            var review = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
            {
                throw new NotFoundException("Yorum bulunamadı.");
            }

            return MapToDto(review);
        }

        public async Task<PagedResult<ReviewDTO>> GetProductReviewsAsync(int productId, int page, int pageSize)
        {
            var query = _context.Reviews
                .Include(r => r.User)
                .Where(r => r.ProductId == productId && r.Status == ReviewStatus.Approved)
                .OrderByDescending(r => r.CreatedDate);

            var totalCount = await query.CountAsync();
            var reviews = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<ReviewDTO>
            {
                Data = reviews.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<PagedResult<ReviewDTO>> GetPendingReviewsAsync(int page, int pageSize)
        {
            var query = _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .Where(r => r.Status == ReviewStatus.Pending)
                .OrderBy(r => r.CreatedDate);

            var totalCount = await query.CountAsync();
            var reviews = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<ReviewDTO>
            {
                Data = reviews.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<bool> ApproveReviewAsync(int reviewId)
        {
            try
            {
                var review = await _context.Reviews.FindAsync(reviewId);
                if (review == null)
                {
                    throw new NotFoundException("Yorum bulunamadı.");
                }

                if (review.Status == ReviewStatus.Approved)
                {
                    return true;
                }

                review.Status = ReviewStatus.Approved;
                review.ApprovedDate = DateTime.UtcNow;
                review.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving review: ReviewId={ReviewId}", reviewId);
                throw;
            }
        }

        public async Task<bool> RejectReviewAsync(int reviewId)
        {
            try
            {
                var review = await _context.Reviews.FindAsync(reviewId);
                if (review == null)
                {
                    throw new NotFoundException("Yorum bulunamadı.");
                }

                review.Status = ReviewStatus.Rejected;
                review.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting review: ReviewId={ReviewId}", reviewId);
                throw;
            }
        }

        public async Task<bool> DeleteReviewAsync(int reviewId, int userId)
        {
            try
            {
                var review = await _context.Reviews.FindAsync(reviewId);
                if (review == null)
                {
                    throw new NotFoundException("Yorum bulunamadı.");
                }

                // ✅ Sadece kendi yorumunu silebilir
                if (review.UserId != userId)
                {
                    throw new UnauthorizedAccessException("Bu yorumu silme yetkiniz yok.");
                }

                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting review: ReviewId={ReviewId}", reviewId);
                throw;
            }
        }

        public async Task<ProductReviewStats> GetProductReviewStatsAsync(int productId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.ProductId == productId && r.Status == ReviewStatus.Approved)
                .ToListAsync();

            if (!reviews.Any())
            {
                return new ProductReviewStats
                {
                    ProductId = productId,
                    TotalReviews = 0,
                    AverageRating = 0,
                    RatingDistribution = new Dictionary<int, int>
                {
                    { 5, 0 }, { 4, 0 }, { 3, 0 }, { 2, 0 }, { 1, 0 }
                }
                };
            }

            return new ProductReviewStats
            {
                ProductId = productId,
                TotalReviews = reviews.Count,
                AverageRating = Math.Round(reviews.Average(r => r.Rating), 1),
                RatingDistribution = reviews
                    .GroupBy(r => r.Rating)
                    .ToDictionary(g => g.Key, g => g.Count())
            };
        }

        public async Task<bool> CanUserReviewProductAsync(int userId, int productId)
        {
            // ✅ Ürünü satın almış mı?
            var hasPurchased = await HasUserPurchasedProductAsync(userId, productId);
            if (!hasPurchased)
            {
                return false;
            }

            // ✅ Daha önce yorum yapmış mı?
            var hasReviewed = await _context.Reviews
                .AnyAsync(r => r.UserId == userId && r.ProductId == productId);

            return !hasReviewed;
        }

        // Private helper methods
        private async Task<bool> HasUserPurchasedProductAsync(int userId, int productId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId && o.Status == OrderStatus.Completed)
                .SelectMany(o => o.OrderItems)
                .AnyAsync(oi => oi.ProductId == productId);
        }

        private ReviewDTO MapToDto(Review review)
        {
            return new ReviewDTO
            {
                Id = review.Id,
                ProductId = review.ProductId,
                ProductName = review.Product?.Name,
                UserId = review.UserId,
                Username = review.User?.Username ?? "Anonim",
                Rating = review.Rating,
                Comment = review.Comment,
                Status = review.Status.ToString(),
                CreatedAt = review.CreatedDate,
                ApprovedAt = review.ApprovedDate
            };
        }
    }
}
