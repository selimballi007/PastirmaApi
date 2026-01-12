using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PastirmaApi.Application.DTOs.CategoryDTOs;
using PastirmaApi.Application.Interfaces.Services;
using PastirmaApi.Core.Exceptions;

namespace PastirmaApi.API.Controllers
{
    // Controllers/CategoriesController.cs
    [ApiController]
    [Route("api/category")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        // POST: api/categories
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CategoryDTO>> CreateCategory([FromBody] CreateCategoryDTO dto)
        {
            try
            {
                var category = await _categoryService.CreateCategoryAsync(dto);
                return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, category);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return StatusCode(500, new { message = "Kategori oluþturulurken bir hata oluþtu." });
            }
        }

        // PUT: api/categories/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CategoryDTO>> UpdateCategory(int id, [FromBody] UpdateCategoryDTO dto)
        {
            try
            {
                var category = await _categoryService.UpdateCategoryAsync(id, dto);
                return Ok(category);
            }
            catch (BusinessException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category: {CategoryId}", id);
                return StatusCode(500, new { message = "Kategori güncellenirken bir hata oluþtu." });
            }
        }

        // DELETE: api/categories/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteCategory(int id)
        {
            try
            {
                await _categoryService.DeleteCategoryAsync(id);
                return Ok(new { message = "Kategori baþarýyla silindi." });
            }
            catch (BusinessException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category: {CategoryId}", id);
                return StatusCode(500, new { message = "Kategori silinirken bir hata oluþtu." });
            }
        }

        // GET: api/categories/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDTO>> GetCategoryById(int id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                return Ok(category);
            }
            catch (BusinessException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category: {CategoryId}", id);
                return StatusCode(500, new { message = "Kategori getirilirken bir hata oluþtu." });
            }
        }

        // GET: api/categories
        [HttpGet]
        public async Task<ActionResult<List<CategoryDTO>>> GetAllCategories([FromQuery] bool includeInactive = false)
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync(includeInactive);
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all categories");
                return StatusCode(500, new { message = "Kategoriler getirilirken bir hata oluþtu." });
            }
        }

        // GET: api/categories/with-product-count
        [HttpGet("with-product-count")]
        public async Task<ActionResult<List<CategoryWithProductCountDTO>>> GetCategoriesWithProductCount()
        {
            try
            {
                var categories = await _categoryService.GetCategoriesWithProductCountAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories with product count");
                return StatusCode(500, new { message = "Kategoriler getirilirken bir hata oluþtu." });
            }
        }

        // PUT: api/categories/reorder
        [HttpPut("reorder")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ReorderCategories([FromBody] List<ReorderCategoryDTO> categories)
        {
            try
            {
                await _categoryService.ReorderCategoriesAsync(categories);
                return Ok(new { message = "Kategori sýralamasý güncellendi." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering categories");
                return StatusCode(500, new { message = "Sýralama güncellenirken bir hata oluþtu." });
            }
        }

        // PUT: api/categories/{id}/toggle-status
        [HttpPut("{id}/toggle-status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ToggleCategoryStatus(int id)
        {
            try
            {
                var isActive = await _categoryService.ToggleCategoryStatusAsync(id);
                return Ok(new
                {
                    message = isActive ? "Kategori aktif edildi." : "Kategori pasif edildi.",
                    isActive
                });
            }
            catch (BusinessException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling category status: {CategoryId}", id);
                return StatusCode(500, new { message = "Durum deðiþtirilirken bir hata oluþtu." });
            }
        }
    }
}
