using PastirmaApi.Application.DTOs.CaptchaDTOs;

namespace PastirmaApi.Infrastructure.GoogleCaptcha
{
    public interface ICaptchaService
    {
        Task<CaptchaVerificationResultDTO> VerifyAsync(string token);
    }
}
