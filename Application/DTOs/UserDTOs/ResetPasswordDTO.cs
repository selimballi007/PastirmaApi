namespace PastirmaApi.Application.DTOs.UserDTOs
{
    public class ResetPasswordDTO
    {
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string CaptchaToken { get; set; } = string.Empty;
    }
}
