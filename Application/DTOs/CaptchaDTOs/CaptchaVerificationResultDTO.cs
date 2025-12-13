namespace PastirmaApi.Application.DTOs.CaptchaDTOs
{
    public class CaptchaVerificationResultDTO
    {
        public bool Success { get; set; }
        public List<string> ErrorCodes { get; set; } = new();
    }
}
