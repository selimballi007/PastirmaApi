using System.ComponentModel.DataAnnotations;

namespace PastirmaApi.Application.DTOs.UserDTOs
{
    public class UpdateProfileDTO
    {
        [Required(ErrorMessage = "Kullanıcı adı zorunludur")]
        [MinLength(3, ErrorMessage = "Kullanıcı adı en az 3 karakter olmalı")]
        [MaxLength(50, ErrorMessage = "Kullanıcı adı en fazla 50 karakter olabilir")]
        public string Username { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "Ad soyad en fazla 100 karakter olabilir")]
        public string? FullName { get; set; }
    }
}
