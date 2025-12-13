using PastirmaApi.Application.DTOs.HeroSlideDTOs;

namespace PastirmaApi.Application.Interfaces.Services
{
    public interface IHeroSlideService
    {
        Task<HeroSlideDTO> CreateSlideAsync(CreateHeroSlideDTO dto);
        Task<HeroSlideDTO> UpdateSlideAsync(int id, UpdateHeroSlideDTO dto);
        Task<bool> DeleteSlideAsync(int id);
        Task<HeroSlideDTO> GetSlideByIdAsync(int id);
        Task<List<HeroSlideDTO>> GetAllSlidesAsync(bool includeInactive = false);
        Task<List<HeroSlideDTO>> GetActiveSlidesAsync();
        Task<bool> ReorderSlidesAsync(List<ReorderSlideDTO> slides);
        Task<bool> ToggleSlideStatusAsync(int id);
    }
}
