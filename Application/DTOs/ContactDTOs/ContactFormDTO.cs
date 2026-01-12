using System.ComponentModel.DataAnnotations;

namespace PastirmaApi.Application.DTOs.ContactDTOs
{
    public class ContactFormDTO
    {
        [Required(ErrorMessage = "Ad Soyad zorunludur")]
        [MinLength(2, ErrorMessage = "Ad Soyad en az 2 karakter olmalıdır")]
        [MaxLength(100, ErrorMessage = "Ad Soyad en fazla 100 karakter olabilir")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Email zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
        public string Email { get; set; } = null!;

        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Konu zorunludur")]
        public string Subject { get; set; } = null!;

        [Required(ErrorMessage = "Mesaj zorunludur")]
        [MinLength(10, ErrorMessage = "Mesaj en az 10 karakter olmalıdır")]
        [MaxLength(1000, ErrorMessage = "Mesaj en fazla 1000 karakter olabilir")]
        public string Message { get; set; } = null!;

        [Required(ErrorMessage = "Captcha token zorunludur")]
        public string CaptchaToken { get; set; } = null!;
    }
}
