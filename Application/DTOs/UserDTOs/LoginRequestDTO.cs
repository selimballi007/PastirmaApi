using System.ComponentModel.DataAnnotations;

namespace PastirmaApi.Application.DTOs.UserDTOs
{
    public class LoginRequestDTO
    {
        [Required(ErrorMessage = "Email zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
        public required string Email { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalı ")]
        public required string PasswordHash { get; set; }
    }
}
