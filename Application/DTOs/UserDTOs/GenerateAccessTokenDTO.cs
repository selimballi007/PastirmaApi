using PastirmaApi.Core.Entities;

namespace PastirmaApi.Application.DTOs.UserDTOs
{
    public record GenerateAccessTokenDTO
    (
        int Id,
        string Email,
        string? Username,
        string Role
    );    
}
