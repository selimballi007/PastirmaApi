using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PastirmaApi.Application.DTOs.ReviewDTO;
using PastirmaApi.Application.DTOs.ReviewDTOs;
using PastirmaApi.Application.Interfaces.Services;
using PastirmaApi.Core.Exceptions;
using System.Security.Claims;

namespace PastirmaApi.API.Controllers
{
    // Controllers/ReviewsController.cs
    [ApiController]
    [Route("api/reviews")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly ILogger<ReviewsController> _logger;

        public ReviewsController(IReviewService reviewService, ILogger<ReviewsController> logger)
        {
            _reviewService = reviewService;
            _logger = logger;
        }

        // POST: api/reviews
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ReviewDTO>> CreateReview([FromBody] CreateReviewDTO dto)
        {
            try
            {
                var userId = GetUserId();
                var review = await _reviewService.CreateReviewAsync(userId, dto);
                return CreatedAtAction(nameof(GetReviewById), new { id = review.Id }, review);
            }
            catch (UnauthorizedAccessException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review");
                return StatusCode(500, new { message = "Yorum oluşturulurken bir hata oluştu." });
            }
        }

        // GET: api/reviews/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ReviewDTO>> GetReviewById(int id)
        {
            try
            {
                var review = await _reviewService.GetReviewByIdAsync(id);
                return Ok(review);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting review: {ReviewId}", id);
                return StatusCode(500, new { message = "Yorum getirilirken bir hata oluştu." });
            }
        }

        // GET: api/reviews/product/{productId}?page=1&pageSize=10
        [HttpGet("product/{productId}")]
        public async Task<ActionResult<PagedResult<ReviewDTO>>> GetProductReviews(
            int productId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 50) pageSize = 10;

                var result = await _reviewService.GetProductReviewsAsync(productId, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product reviews: {ProductId}", productId);
                return StatusCode(500, new { message = "Yorumlar getirilirken bir hata oluştu." });
            }
        }

        // GET: api/reviews/product/{productId}/stats
        [HttpGet("product/{productId}/stats")]
        public async Task<ActionResult<ProductReviewStats>> GetProductReviewStats(int productId)
        {
            try
            {
                var stats = await _reviewService.GetProductReviewStatsAsync(productId);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product review stats: {ProductId}", productId);
                return StatusCode(500, new { message = "İstatistikler getirilirken bir hata oluştu." });
            }
        }

        // GET: api/reviews/stats
        [HttpGet("stats")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> GetReviewStats()
        {
            try
            {
                var stats = await _reviewService.GetReviewStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting review stats");
                return StatusCode(500, new { message = "İstatistikler getirilirken bir hata oluştu." });
            }
        }

        // GET: api/reviews?page=1&pageSize=10
        [HttpGet]
        public async Task<ActionResult<List<ReviewDTO>>> GetAllApprovedReviews(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 1000) pageSize = 10;

                var result = await _reviewService.GetApprovedReviewsAsync(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all approved reviews");
                return StatusCode(500, new { message = "Onaylanmış yorumlar getirilirken bir hata oluştu." });
            }
        }

        // GET: api/reviews/pending?page=1&pageSize=10
        [HttpGet("pending")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PagedResult<ReviewDTO>>> GetPendingReviews(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 50) pageSize = 10;

                var result = await _reviewService.GetPendingReviewsAsync(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending reviews");
                return StatusCode(500, new { message = "Bekleyen yorumlar getirilirken bir hata oluştu." });
            }
        }

        // GET: api/reviews/all?page=1&pageSize=10&status=Approved
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PagedResult<ReviewDTO>>> GetAllReviews(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? status = null)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 50) pageSize = 10;

                var result = await _reviewService.GetAllReviewsByStatusAsync(page, pageSize, status);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reviews by status: {Status}", status);
                return StatusCode(500, new { message = "Yorumlar getirilirken bir hata oluştu." });
            }
        }

        // PUT: api/reviews/{id}/approve
        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ApproveReview(int id)
        {
            try
            {
                var result = await _reviewService.ApproveReviewAsync(id);

                if (result)
                {
                    return Ok(new { message = "Yorum onaylandı." });
                }

                return BadRequest(new { message = "Yorum onaylanamadı." });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving review: {ReviewId}", id);
                return StatusCode(500, new { message = "Yorum onaylanırken bir hata oluştu." });
            }
        }

        // PUT: api/reviews/{id}/reject
        [HttpPut("{id}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> RejectReview(int id)
        {
            try
            {
                var result = await _reviewService.RejectReviewAsync(id);

                if (result)
                {
                    return Ok(new { message = "Yorum reddedildi." });
                }

                return BadRequest(new { message = "Yorum reddedilemedi." });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting review: {ReviewId}", id);
                return StatusCode(500, new { message = "Yorum reddedilirken bir hata oluştu." });
            }
        }

        // DELETE: api/reviews/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteReview(int id)
        {
            try
            {
                var userId = GetUserId();
                var result = await _reviewService.DeleteReviewAsync(id, userId);

                if (result)
                {
                    return Ok(new { message = "Yorum silindi." });
                }

                return BadRequest(new { message = "Yorum silinemedi." });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException )
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting review: {ReviewId}", id);
                return StatusCode(500, new { message = "Yorum silinirken bir hata oluştu." });
            }
        }

        // GET: api/reviews/can-review/{productId}
        [HttpGet("can-review/{productId}")]
        [Authorize]
        public async Task<ActionResult<bool>> CanUserReviewProduct(int productId)
        {
            try
            {
                var userId = GetUserId();
                var canReview = await _reviewService.CanUserReviewProductAsync(userId, productId);
                return Ok(new { canReview });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user can review product: {ProductId}", productId);
                return StatusCode(500, new { message = "Kontrol sırasında bir hata oluştu." });
            }
        }

        // Helper method to get user ID from JWT token
        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("Geçersiz kullanıcı.");
            }
            return userId;
        }
    }
}
