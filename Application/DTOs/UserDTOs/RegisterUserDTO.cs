using PastirmaApi.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace PastirmaApi.Application.DTOs.UserDTOs
{
    public class RegisterUserDTO
    {
        [Required(ErrorMessage = "Kullanıcı adı zorunludur")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "Email zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz")]
        public string Email { get; set; } = null!;

        [Required]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalı ")]
        public string PasswordHash { get; set; } = null!;
        public UserRole Role { get; set; } = UserRole.Customer;
    }
}
