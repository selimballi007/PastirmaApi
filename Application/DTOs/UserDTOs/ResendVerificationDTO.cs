namespace PastirmaApi.Application.DTOs.UserDTOs
{
    public class ResendVerificationDTO
    {
        public string Token { get; set; } = string.Empty;
        public string CaptchaToken { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
