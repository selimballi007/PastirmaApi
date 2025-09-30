namespace PastirmaApi.Application.DTOs.UserDTOs
{
    public class ForgotPasswordDTO
    {
        public string Email { get; set; } = string.Empty;
        public string CaptchaToken { get; set; } = string.Empty;
    }
}
