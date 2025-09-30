using PastirmaApi.Application.DTOs.UserDTOs;

namespace PastirmaApi.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task RegisterUserAsync(RegisterUserDTO dto);
        Task<LoginResponseDTO> LoginUserAsync(LoginUserDTO dto);
        Task<bool> UserExistsAsync(string email);
        Task VerifyEmailAsync(string token);
        Task ResendVerificationByTokenAsync(string token);
        Task ResendVerificationByEmailAsync(string email);
    }
}
