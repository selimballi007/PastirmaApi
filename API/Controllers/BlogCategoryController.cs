using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PastirmaApi.Application.DTOs.BlogCategoryDTOs;
using PastirmaApi.Application.Interfaces.Services;

namespace PastirmaApi.API.Controllers
{
    [ApiController]
    [Route("api/blog-category")]
    public class BlogCategoryController : ControllerBase
    {
        private readonly IBlogCategoryService _blogCategoryService;

        public BlogCategoryController(IBlogCategoryService blogCategoryService)
        {
            _blogCategoryService = blogCategoryService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BlogCategoryDTO>> CreateCategory([FromBody] CreateBlogCategoryDTO dto)
        {
            var category = await _blogCategoryService.CreateCategoryAsync(dto);
            return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, category);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BlogCategoryDTO>> UpdateCategory(int id, [FromBody] CreateBlogCategoryDTO dto)
        {
            var category = await _blogCategoryService.UpdateCategoryAsync(id, dto);
            return Ok(category);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteCategory(int id)
        {
            await _blogCategoryService.DeleteCategoryAsync(id);
            return Ok(new { message = "Kategori başarıyla silindi." });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BlogCategoryDTO>> GetCategoryById(int id)
        {
            var category = await _blogCategoryService.GetCategoryByIdAsync(id);
            return Ok(category);
        }

        [HttpGet]
        public async Task<ActionResult<List<BlogCategoryDTO>>> GetAllCategories([FromQuery] bool includeInactive = false)
        {
            var categories = await _blogCategoryService.GetAllCategoriesAsync(includeInactive);
            return Ok(categories);
        }

        [HttpGet("active")]
        public async Task<ActionResult<List<BlogCategoryDTO>>> GetActiveCategories()
        {
            var categories = await _blogCategoryService.GetActiveCategoriesAsync();
            return Ok(categories);
        }
    }
}
