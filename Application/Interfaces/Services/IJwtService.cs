using PastirmaApi.Core.Entities;
using System.Security.Claims;

namespace PastirmaApi.Application.Interfaces.Services
{
    public interface IJwtService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        string GenerateEmailVerificationToken(string email);
        string ValidateEmailVerificationToken(string token);
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
        string GeneratePasswordResetToken(string email);
        string? ValidatePasswordResetToken(string token);
    }
}
