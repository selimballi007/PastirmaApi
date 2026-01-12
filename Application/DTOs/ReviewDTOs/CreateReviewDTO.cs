using System.ComponentModel.DataAnnotations;

namespace PastirmaApi.Application.DTOs.ReviewDTOs
{
    public class CreateReviewDTO
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating 1-5 arasında olmalıdır.")]
        public int Rating { get; set; }

        [MaxLength(1000, ErrorMessage = "Yorum en fazla 1000 karakter olabilir.")]
        public string? Comment { get; set; }
    }
}
