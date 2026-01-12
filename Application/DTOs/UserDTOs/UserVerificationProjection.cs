namespace PastirmaApi.Application.DTOs.UserDTOs
{
    public record UserVerificationProjection
    (
        int Id,
        bool IsVerified
    );    
}
