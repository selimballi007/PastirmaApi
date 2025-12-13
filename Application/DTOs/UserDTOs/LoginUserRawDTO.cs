using PastirmaApi.Core.Entities;

namespace PastirmaApi.Application.DTOs.UserDTOs
{
    internal class LoginUserRawDTO
    {
        public int id { get; set; }
        public required string username { get; set; }
        public required string email { get; set; }
        public UserRole role { get; set; }
        public required string password_hash { get; set; }
        public bool is_verified { get; set; }
        public DateTime? last_login_at { get; set; }
        public string? refresh_token { get; set; }
        public DateTime? refresh_token_expiry { get; set; }
    }
}