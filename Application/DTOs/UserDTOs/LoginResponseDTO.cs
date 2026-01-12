namespace PastirmaApi.Application.DTOs.UserDTOs
{
    public record LoginResponseDTO
    (
        int id,
        string? username,
        string email,
        string role,
        DateTime? lastLoginAt
    );
    
    public record LoginTransDTO
    (
        int id,
        string? userName,
        string email,
        string role,
        string accessToken,
        string refreshToken,
        DateTime? refreshTokenExpiry,
        DateTime? lastLoginAt
    );    
}
