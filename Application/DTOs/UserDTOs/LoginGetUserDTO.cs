using PastirmaApi.Core.Entities;

namespace PastirmaApi.Application.DTOs.UserDTOs
{
    public record LoginGetUserDTO
    (
        int Id,
        string Username,
        string Email,
        UserRole Role,
        string PasswordHash,
        DateTime? LastLoginAt,
        string? RefreshToken,
        DateTime? RefreshTokenExpiry,
        bool IsVerified
    );
}
