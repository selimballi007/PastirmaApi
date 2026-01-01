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
                // ? Kullanýcý bu ürünü satýn almýþ mý?
                var hasPurchased = await HasUserPurchasedProductAsync(userId, dto.ProductId);
                if (!hasPurchased)
                {
                    throw new UnauthorizedAccessException("Bu ürünü satýn almadan yorum yapamazsýnýz.");
                }

                // ? Daha önce yorum yapmýþ mý?
                var existingReview = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.UserId == userId && r.ProductId == dto.ProductId);

                if (existingReview != null)
                {
                    throw new InvalidOperationException("Bu ürün için zaten yorum yaptýnýz.");
                }

                // ? Ürün var mý?
                var product = await _context.Products.FindAsync(dto.ProductId);
                if (product == null)
                {
                    throw new NotFoundException("Ürün bulunamadý.");
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
                throw new NotFoundException("Yorum bulunamadý.");
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

        public async Task<List<ReviewDTO>> GetApprovedReviewsAsync(int page, int pageSize)
        {
            var query = _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .Where(r => r.Status == ReviewStatus.Approved)
                .OrderByDescending(r => r.CreatedDate);

            var reviews = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return reviews.Select(MapToDto).ToList();
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

        public async Task<PagedResult<ReviewDTO>> GetAllReviewsByStatusAsync(int page, int pageSize, string? status)
        {
            var query = _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .AsQueryable();

            // Filter by status if provided
            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                if (Enum.TryParse<ReviewStatus>(status, true, out var reviewStatus))
                {
                    query = query.Where(r => r.Status == reviewStatus);
                }
            }

            query = query.OrderByDescending(r => r.CreatedDate);

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
                    throw new NotFoundException("Yorum bulunamadý.");
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
                    throw new NotFoundException("Yorum bulunamadý.");
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
                    throw new NotFoundException("Yorum bulunamadý.");
                }

                // ? Sadece kendi yorumunu silebilir
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

        /// <summary>
        /// Get global review statistics for dashboard
        /// Returns counts of reviews by status (Pending, Approved, Rejected)
        /// </summary>
        public async Task<ReviewStats> GetReviewStatsAsync()
        {
            var pendingCount = await _context.Reviews
                .CountAsync(r => r.Status == ReviewStatus.Pending);

            var approvedCount = await _context.Reviews
                .CountAsync(r => r.Status == ReviewStatus.Approved);

            var rejectedCount = await _context.Reviews
                .CountAsync(r => r.Status == ReviewStatus.Rejected);

            return new ReviewStats
            {
                Pending = pendingCount,
                Approved = approvedCount,
                Rejected = rejectedCount
            };
        }

        public async Task<bool> CanUserReviewProductAsync(int userId, int productId)
        {
            // ? Ürünü satýn almýþ mý?
            var hasPurchased = await HasUserPurchasedProductAsync(userId, productId);
            if (!hasPurchased)
            {
                return false;
            }

            // ? Daha önce yorum yapmýþ mý?
            var hasReviewed = await _context.Reviews
                .AnyAsync(r => r.UserId == userId && r.ProductId == productId);

            return !hasReviewed;
        }

        // Private helper methods
        private async Task<bool> HasUserPurchasedProductAsync(int userId, int productId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId && o.Status == OrderStatus.Delivered)
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
