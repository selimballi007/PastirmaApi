namespace PastirmaApi.Application.DTOs.UserDTOs
{
    public record RefreshTokenDTO
    {
        public DateTime last_login_at { get; set; }
    }
}
