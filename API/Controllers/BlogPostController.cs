using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PastirmaApi.Application.DTOs.BlogPostDTOs;
using PastirmaApi.Application.Interfaces.Services;
using PastirmaApi.Core.Exceptions;
using System.Security.Claims;

namespace PastirmaApi.API.Controllers
{
    [ApiController]
    [Route("api/blog-post")]
    public class BlogPostController : ControllerBase
    {
        private readonly IBlogPostService _blogPostService;
        private readonly ILogger<BlogPostController> _logger;

        public BlogPostController(IBlogPostService blogPostService, ILogger<BlogPostController> logger)
        {
            _blogPostService = blogPostService;
            _logger = logger;
        }

        // POST: api/BlogPost
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BlogPostDTO>> CreatePost([FromBody] CreateBlogPostDTO dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var post = await _blogPostService.CreatePostAsync(dto, userId);
                return CreatedAtAction(nameof(GetPostById), new { id = post.Id }, post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating blog post");
                return StatusCode(500, new { message = "Blog yazısı oluşturulurken bir hata oluştu." });
            }
        }

        // PUT: api/BlogPost/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BlogPostDTO>> UpdatePost(int id, [FromBody] UpdateBlogPostDTO dto)
        {
            try
            {
                var post = await _blogPostService.UpdatePostAsync(id, dto);
                return Ok(post);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating blog post: {PostId}", id);
                return StatusCode(500, new { message = "Blog yazısı güncellenirken bir hata oluştu." });
            }
        }

        // DELETE: api/BlogPost/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeletePost(int id)
        {
            try
            {
                await _blogPostService.DeletePostAsync(id);
                return Ok(new { message = "Blog yazısı başarıyla silindi." });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting blog post: {PostId}", id);
                return StatusCode(500, new { message = "Blog yazısı silinirken bir hata oluştu." });
            }
        }

        // GET: api/BlogPost/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<BlogPostDTO>> GetPostById(int id, [FromQuery] bool incrementView = false)
        {
            try
            {
                var post = await _blogPostService.GetPostByIdAsync(id, incrementView);
                return Ok(post);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting blog post: {PostId}", id);
                return StatusCode(500, new { message = "Blog yazısı getirilirken bir hata oluştu." });
            }
        }

        // GET: api/BlogPost
        [HttpGet]
        public async Task<ActionResult<List<BlogPostListDTO>>> GetAllPosts([FromQuery] bool includeInactive = false)
        {
            try
            {
                var posts = await _blogPostService.GetAllPostsAsync(includeInactive);
                return Ok(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all blog posts");
                return StatusCode(500, new { message = "Blog yazıları getirilirken bir hata oluştu." });
            }
        }

        // GET: api/BlogPost/published
        [HttpGet("published")]
        public async Task<ActionResult<List<BlogPostListDTO>>> GetPublishedPosts()
        {
            try
            {
                var posts = await _blogPostService.GetPublishedPostsAsync();
                return Ok(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting published blog posts");
                return StatusCode(500, new { message = "Yayınlanmış blog yazıları getirilirken bir hata oluştu." });
            }
        }

        // GET: api/BlogPost/featured
        [HttpGet("featured")]
        public async Task<ActionResult<List<BlogPostListDTO>>> GetFeaturedPosts()
        {
            try
            {
                var posts = await _blogPostService.GetFeaturedPostsAsync();
                return Ok(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting featured blog posts");
                return StatusCode(500, new { message = "Öne çıkan blog yazıları getirilirken bir hata oluştu." });
            }
        }

        // PUT: api/BlogPost/{id}/toggle-status
        [HttpPut("{id}/toggle-status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> TogglePostStatus(int id)
        {
            try
            {
                var isActive = await _blogPostService.TogglePostStatusAsync(id);
                return Ok(new
                {
                    message = isActive ? "Blog yazısı aktif edildi." : "Blog yazısı pasif edildi.",
                    isActive
                });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling blog post status: {PostId}", id);
                return StatusCode(500, new { message = "Durum değiştirilirken bir hata oluştu." });
            }
        }

        // PUT: api/BlogPost/{id}/toggle-featured
        [HttpPut("{id}/toggle-featured")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ToggleFeatured(int id)
        {
            try
            {
                var isFeatured = await _blogPostService.ToggleFeaturedAsync(id);
                return Ok(new
                {
                    message = isFeatured ? "Blog yazısı öne çıkarıldı." : "Blog yazısı öne çıkarılmaktan kaldırıldı.",
                    isFeatured
                });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling blog post featured status: {PostId}", id);
                return StatusCode(500, new { message = "Öne çıkarma durumu değiştirilirken bir hata oluştu." });
            }
        }

        // Helper method to get current user ID from JWT claims
        private int GetCurrentUserId()
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
