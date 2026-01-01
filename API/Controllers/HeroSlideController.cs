using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PastirmaApi.Application.DTOs.HeroSlideDTOs;
using PastirmaApi.Application.Interfaces.Services;
using PastirmaApi.Core.Exceptions;

namespace PastirmaApi.API.Controllers
{
    [ApiController]
    [Route("api/hero-slide")]
    public class HeroSlideController : ControllerBase
    {
        private readonly IHeroSlideService _heroSlideService;
        private readonly ILogger<HeroSlideController> _logger;

        public HeroSlideController(IHeroSlideService heroSlideService, ILogger<HeroSlideController> logger)
        {
            _heroSlideService = heroSlideService;
            _logger = logger;
        }

        // POST: api/hero-slides
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<HeroSlideDTO>> CreateSlide([FromBody] CreateHeroSlideDTO dto)
        {
            try
            {
                var slide = await _heroSlideService.CreateSlideAsync(dto);
                return CreatedAtAction(nameof(GetSlideById), new { id = slide.Id }, slide);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating hero slide");
                return StatusCode(500, new { message = "Slide oluşturulurken bir hata oluştu." });
            }
        }

        // PUT: api/hero-slides/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<HeroSlideDTO>> UpdateSlide(int id, [FromBody] UpdateHeroSlideDTO dto)
        {
            try
            {
                var slide = await _heroSlideService.UpdateSlideAsync(id, dto);
                return Ok(slide);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating hero slide: {SlideId}", id);
                return StatusCode(500, new { message = "Slide güncellenirken bir hata oluştu." });
            }
        }

        // DELETE: api/hero-slides/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteSlide(int id)
        {
            try
            {
                await _heroSlideService.DeleteSlideAsync(id);
                return Ok(new { message = "Slide başarıyla silindi." });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting hero slide: {SlideId}", id);
                return StatusCode(500, new { message = "Slide silinirken bir hata oluştu." });
            }
        }

        // GET: api/hero-slides/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<HeroSlideDTO>> GetSlideById(int id)
        {
            try
            {
                var slide = await _heroSlideService.GetSlideByIdAsync(id);
                return Ok(slide);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hero slide: {SlideId}", id);
                return StatusCode(500, new { message = "Slide getirilirken bir hata oluştu." });
            }
        }

        // GET: api/hero-slides
        [HttpGet]
        public async Task<ActionResult<List<HeroSlideDTO>>> GetAllSlides([FromQuery] bool includeInactive = false)
        {
            try
            {
                var slides = await _heroSlideService.GetAllSlidesAsync(includeInactive);
                return Ok(slides);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all hero slides");
                return StatusCode(500, new { message = "Slide'lar getirilirken bir hata oluştu." });
            }
        }

        // GET: api/hero-slides/active
        [HttpGet("active")]
        public async Task<ActionResult<List<HeroSlideDTO>>> GetActiveSlides()
        {
            try
            {
                var slides = await _heroSlideService.GetActiveSlidesAsync();
                return Ok(slides);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active hero slides");
                return StatusCode(500, new { message = "Aktif slide'lar getirilirken bir hata oluştu." });
            }
        }

        // PUT: api/hero-slides/reorder
        [HttpPut("reorder")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ReorderSlides([FromBody] List<ReorderSlideDTO> slides)
        {
            try
            {
                await _heroSlideService.ReorderSlidesAsync(slides);
                return Ok(new { message = "Slide sıralaması güncellendi." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering hero slides");
                return StatusCode(500, new { message = "Sıralama güncellenirken bir hata oluştu." });
            }
        }

        // PUT: api/hero-slides/{id}/toggle-status
        [HttpPut("{id}/toggle-status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ToggleSlideStatus(int id)
        {
            try
            {
                var isActive = await _heroSlideService.ToggleSlideStatusAsync(id);
                return Ok(new
                {
                    message = isActive ? "Slide aktif edildi." : "Slide pasif edildi.",
                    isActive
                });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling hero slide status: {SlideId}", id);
                return StatusCode(500, new { message = "Durum değiştirilirken bir hata oluştu." });
            }
        }
    }
}
