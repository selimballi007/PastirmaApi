namespace PastirmaApi.Application.DTOs.UserDTOs
{
    public record UpdateTokenDTO
    (        
        int Id,
        string oldRefreshToken,
        string newRefreshToken,
        DateTime newRefreshTokenExpiry
    );
}
