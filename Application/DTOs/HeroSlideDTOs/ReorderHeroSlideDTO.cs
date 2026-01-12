using System.ComponentModel.DataAnnotations;

namespace PastirmaApi.Application.DTOs.HeroSlideDTOs
{
    public class ReorderSlideDTO
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int DisplayOrder { get; set; }
    }
}
