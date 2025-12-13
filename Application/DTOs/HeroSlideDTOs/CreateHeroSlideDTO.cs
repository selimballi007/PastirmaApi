using System.ComponentModel.DataAnnotations;

namespace PastirmaApi.Application.DTOs.HeroSlideDTOs
{
    public class CreateHeroSlideDTO
    {
        [Required(ErrorMessage = "Başlık zorunludur.")]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Subtitle { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? Discount { get; set; }

        [Required(ErrorMessage = "Görsel URL zorunludur.")]
        public string ImageUrl { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ButtonText { get; set; } = "Hemen Keşfet";

        [Required]
        [MaxLength(200)]
        public string ButtonLink { get; set; } = "/products";

        [Required]
        [MaxLength(100)]
        public string BgColor { get; set; } = "from-amber-600 to-orange-700";
    }
}
