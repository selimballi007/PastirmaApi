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

        public BlogPostController(IBlogPostService blogPostService)
        {
            _blogPostService = blogPostService;
        }

        // POST: api/BlogPost
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BlogPostDTO>> CreatePost([FromBody] CreateBlogPostDTO dto)
        {
            var userId = GetCurrentUserId();
            var post = await _blogPostService.CreatePostAsync(dto, userId);
            return CreatedAtAction(nameof(GetPostById), new { id = post.Id }, post);
        }

        // PUT: api/BlogPost/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BlogPostDTO>> UpdatePost(int id, [FromBody] UpdateBlogPostDTO dto)
        {
            var post = await _blogPostService.UpdatePostAsync(id, dto);
            return Ok(post);
        }

        // DELETE: api/BlogPost/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeletePost(int id)
        {
            await _blogPostService.DeletePostAsync(id);
            return Ok(new { message = "Blog yazısı başarıyla silindi." });
        }

        // GET: api/BlogPost/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<BlogPostDTO>> GetPostById(int id, [FromQuery] bool incrementView = false)
        {
            var post = await _blogPostService.GetPostByIdAsync(id, incrementView);
            return Ok(post);
        }

        // GET: api/BlogPost
        [HttpGet]
        public async Task<ActionResult<List<BlogPostListDTO>>> GetAllPosts([FromQuery] bool includeInactive = false)
        {
            var posts = await _blogPostService.GetAllPostsAsync(includeInactive);
            return Ok(posts);
        }

        // GET: api/BlogPost/published
        [HttpGet("published")]
        public async Task<ActionResult<List<BlogPostListDTO>>> GetPublishedPosts()
        {
            var posts = await _blogPostService.GetPublishedPostsAsync();
            return Ok(posts);
        }

        // GET: api/BlogPost/featured
        [HttpGet("featured")]
        public async Task<ActionResult<List<BlogPostListDTO>>> GetFeaturedPosts()
        {
            var posts = await _blogPostService.GetFeaturedPostsAsync();
            return Ok(posts);
        }

        // PUT: api/BlogPost/{id}/toggle-status
        [HttpPut("{id}/toggle-status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> TogglePostStatus(int id)
        {
            var isActive = await _blogPostService.TogglePostStatusAsync(id);
            return Ok(new
            {
                message = isActive ? "Blog yazısı aktif edildi." : "Blog yazısı pasif edildi.",
                isActive
            });
        }

        // PUT: api/BlogPost/{id}/toggle-featured
        [HttpPut("{id}/toggle-featured")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ToggleFeatured(int id)
        {
            var isFeatured = await _blogPostService.ToggleFeaturedAsync(id);
            return Ok(new
            {
                message = isFeatured ? "Blog yazısı öne çıkarıldı." : "Blog yazısı öne çıkarılmaktan kaldırıldı.",
                isFeatured
            });
        }

        // Helper method to get current user ID from JWT claims
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new BusinessException("Geçersiz kullanıcı.");
            }
            return userId;
        }
    }
}
