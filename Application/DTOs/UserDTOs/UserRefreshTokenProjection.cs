using PastirmaApi.Core.Entities;

namespace PastirmaApi.Application.DTOs.UserDTOs
{
    public record UserRefreshTokenProjection
    (
        int Id,
        string? Username,
        string Email,
        UserRole Role,
        DateTime? LastLoginAt
    );
}
