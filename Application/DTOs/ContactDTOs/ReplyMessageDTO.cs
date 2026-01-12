using System.ComponentModel.DataAnnotations;

namespace PastirmaApi.Application.DTOs.ContactDTOs
{
    public class ReplyMessageDTO
    {
        [Required(ErrorMessage = "Konu zorunludur")]
        [MaxLength(200, ErrorMessage = "Konu en fazla 200 karakter olabilir")]
        public string Subject { get; set; } = null!;

        [Required(ErrorMessage = "Mesaj zorunludur")]
        [MinLength(10, ErrorMessage = "Mesaj en az 10 karakter olmalıdır")]
        [MaxLength(5000, ErrorMessage = "Mesaj en fazla 5000 karakter olabilir")]
        public string Message { get; set; } = null!;
    }
}
