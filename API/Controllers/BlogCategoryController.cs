using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PastirmaApi.Application.DTOs.BlogCategoryDTOs;
using PastirmaApi.Application.Interfaces.Services;
using PastirmaApi.Core.Exceptions;

namespace PastirmaApi.API.Controllers
{
    [ApiController]
    [Route("api/blog-category")]
    public class BlogCategoryController : ControllerBase
    {
        private readonly IBlogCategoryService _blogCategoryService;
        private readonly ILogger<BlogCategoryController> _logger;

        public BlogCategoryController(IBlogCategoryService blogCategoryService, ILogger<BlogCategoryController> logger)
        {
            _blogCategoryService = blogCategoryService;
            _logger = logger;
        }

        // POST: api/BlogCategory
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BlogCategoryDTO>> CreateCategory([FromBody] CreateBlogCategoryDTO dto)
        {
            try
            {
                var category = await _blogCategoryService.CreateCategoryAsync(dto);
                return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating blog category");
                return StatusCode(500, new { message = "Kategori oluşturulurken bir hata oluştu." });
            }
        }

        // PUT: api/BlogCategory/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BlogCategoryDTO>> UpdateCategory(int id, [FromBody] CreateBlogCategoryDTO dto)
        {
            try
            {
                var category = await _blogCategoryService.UpdateCategoryAsync(id, dto);
                return Ok(category);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating blog category: {CategoryId}", id);
                return StatusCode(500, new { message = "Kategori güncellenirken bir hata oluştu." });
            }
        }

        // DELETE: api/BlogCategory/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteCategory(int id)
        {
            try
            {
                await _blogCategoryService.DeleteCategoryAsync(id);
                return Ok(new { message = "Kategori başarıyla silindi." });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (BusinessException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting blog category: {CategoryId}", id);
                return StatusCode(500, new { message = "Kategori silinirken bir hata oluştu." });
            }
        }

        // GET: api/BlogCategory/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<BlogCategoryDTO>> GetCategoryById(int id)
        {
            try
            {
                var category = await _blogCategoryService.GetCategoryByIdAsync(id);
                return Ok(category);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting blog category: {CategoryId}", id);
                return StatusCode(500, new { message = "Kategori getirilirken bir hata oluştu." });
            }
        }

        // GET: api/BlogCategory
        [HttpGet]
        public async Task<ActionResult<List<BlogCategoryDTO>>> GetAllCategories([FromQuery] bool includeInactive = false)
        {
            try
            {
                var categories = await _blogCategoryService.GetAllCategoriesAsync(includeInactive);
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all blog categories");
                return StatusCode(500, new { message = "Kategoriler getirilirken bir hata oluştu." });
            }
        }

        // GET: api/BlogCategory/active
        [HttpGet("active")]
        public async Task<ActionResult<List<BlogCategoryDTO>>> GetActiveCategories()
        {
            try
            {
                var categories = await _blogCategoryService.GetActiveCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active blog categories");
                return StatusCode(500, new { message = "Aktif kategoriler getirilirken bir hata oluştu." });
            }
        }
    }
}
