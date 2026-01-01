using Microsoft.EntityFrameworkCore;
using PastirmaApi.Application.DTOs.HeroSlideDTOs;
using PastirmaApi.Application.Interfaces.Services;
using PastirmaApi.Core.Entities;
using PastirmaApi.Core.Exceptions;
using PastirmaApi.Infrastructure.Data;

namespace PastirmaApi.Application.Services
{
    public class HeroSlideService : IHeroSlideService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HeroSlideService> _logger;

        public HeroSlideService(ApplicationDbContext context, ILogger<HeroSlideService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<HeroSlideDTO> CreateSlideAsync(CreateHeroSlideDTO dto)
        {
            try
            {
                // ✅ En yüksek DisplayOrder'ı bul
                var maxOrder = await _context.HeroSlides
                    .MaxAsync(s => (int?)s.DisplayOrder) ?? 0;

                var slide = new HeroSlide
                {
                    Title = dto.Title,
                    Subtitle = dto.Subtitle,
                    Description = dto.Description,
                    Discount = dto.Discount,
                    ImageUrl = dto.ImageUrl,
                    ButtonText = dto.ButtonText,
                    ButtonLink = dto.ButtonLink,
                    BgColor = dto.BgColor,
                    DisplayOrder = maxOrder + 1
                };

                _context.HeroSlides.Add(slide);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Hero slide created: {SlideId}, Title: {Title}",
                    slide.Id, slide.Title);

                return MapToDto(slide);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating hero slide: {Title}", dto.Title);
                throw;
            }
        }

        public async Task<HeroSlideDTO> UpdateSlideAsync(int id, UpdateHeroSlideDTO dto)
        {
            try
            {
                var slide = await _context.HeroSlides.FindAsync(id);
                if (slide == null)
                {
                    throw new NotFoundException("Slide bulunamadı.");
                }

                slide.Title = dto.Title;
                slide.Subtitle = dto.Subtitle;
                slide.Description = dto.Description;
                slide.Discount = dto.Discount;
                slide.ImageUrl = dto.ImageUrl;
                slide.ButtonText = dto.ButtonText;
                slide.ButtonLink = dto.ButtonLink;
                slide.BgColor = dto.BgColor;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Hero slide updated: {SlideId}, Title: {Title}", id, dto.Title);

                return MapToDto(slide);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating hero slide: {SlideId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteSlideAsync(int id)
        {
            try
            {
                var slide = await _context.HeroSlides.FindAsync(id);
                if (slide == null)
                {
                    throw new NotFoundException("Slide bulunamadı.");
                }

                _context.HeroSlides.Remove(slide);
                await _context.SaveChangesAsync();

                // Renumber remaining slides sequentially
                var remainingSlides = await _context.HeroSlides
                    .OrderBy(s => s.DisplayOrder)
                    .ToListAsync();

                for (int i = 0; i < remainingSlides.Count; i++)
                {
                    remainingSlides[i].DisplayOrder = i + 1;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Hero slide deleted: {SlideId}, Title: {Title}. Remaining slides renumbered.",
                    id, slide.Title);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting hero slide: {SlideId}", id);
                throw;
            }
        }

        public async Task<HeroSlideDTO> GetSlideByIdAsync(int id)
        {
            var slide = await _context.HeroSlides.FindAsync(id);

            if (slide == null)
            {
                throw new NotFoundException("Slide bulunamadı.");
            }

            return MapToDto(slide);
        }

        public async Task<List<HeroSlideDTO>> GetAllSlidesAsync(bool includeInactive = false)
        {
            var query = _context.HeroSlides.AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(s => s.IsActive);
            }

            var slides = await query
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();

            return slides.Select(MapToDto).ToList();
        }

        public async Task<List<HeroSlideDTO>> GetActiveSlidesAsync()
        {
            var slides = await _context.HeroSlides
                .Where(s => s.IsActive)
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();

            return slides.Select(MapToDto).ToList();
        }

        public async Task<bool> ReorderSlidesAsync(List<ReorderSlideDTO> slides)
        {
            try
            {
                foreach (var item in slides)
                {
                    var slide = await _context.HeroSlides.FindAsync(item.Id);
                    if (slide != null)
                    {
                        slide.DisplayOrder = item.DisplayOrder;
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Hero slides reordered: {Count} slides", slides.Count);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering hero slides");
                throw;
            }
        }

        public async Task<bool> ToggleSlideStatusAsync(int id)
        {
            try
            {
                var slide = await _context.HeroSlides.FindAsync(id);
                if (slide == null)
                {
                    throw new NotFoundException("Slide bulunamadı.");
                }

                slide.IsActive = !slide.IsActive;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Hero slide status toggled: {SlideId}, IsActive: {IsActive}",
                    id, slide.IsActive);

                return slide.IsActive;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling hero slide status: {SlideId}", id);
                throw;
            }
        }

        // Private helper method
        private HeroSlideDTO MapToDto(HeroSlide slide)
        {
            return new HeroSlideDTO
            {
                Id = slide.Id,
                Title = slide.Title,
                Subtitle = slide.Subtitle,
                Description = slide.Description,
                Discount = slide.Discount,
                ImageUrl = slide.ImageUrl,
                ButtonText = slide.ButtonText,
                ButtonLink = slide.ButtonLink,
                BgColor = slide.BgColor,
                DisplayOrder = slide.DisplayOrder,
                IsActive = slide.IsActive,
                CreatedAt = slide.CreatedDate,
                UpdatedAt = slide.UpdatedDate
            };
        }
    }
}
