using PastirmaApi.Application.DTOs.UserDTOs;
using PastirmaApi.Core.Entities;

namespace PastirmaApi.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<LoginGetUserDTO?> GetByEmailAsync(string email);
        Task<DateTime> UpdateTokenAsync(UpdateTokenDTO dto);
        Task<UserVerificationProjection?> GetVerificationStatusByEmailAsync(string email);
        Task MarkAsVerifiedAsync(int id);
        Task<bool> EmailExistsAsync(string email);
        Task<int> LogoutAsync(int id);
        Task<User> AddAsync(User user);
        Task<int> GetIdByEmailAsync(string email);
        Task UpdatePasswordAsync(int userId, string passwordHash);
        Task<UserRefreshTokenProjection?> GetByRefreshTokenAsync(string refreshToken, int userId);
        Task<LoginGetUserDTO?> GetAndUpdateLoginAsync(string email, string refreshToken, DateTime refreshTokenExpiry);
    }
}
