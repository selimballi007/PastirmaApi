using PastirmaApi.API.Middlewares;
using PastirmaApi.Application.DTOs.CaptchaDTOs;

namespace PastirmaApi.Infrastructure.GoogleCaptcha
{
    public class GoogleRecaptchaService : ICaptchaService
    {
        private readonly HttpClient _httpClient;
        private readonly string _secretKey;
        private readonly ILogger<GoogleRecaptchaService> _logger;

        public GoogleRecaptchaService(
            HttpClient httpClient,
            IConfiguration config,
            ILogger<GoogleRecaptchaService> logger)
        {
            _httpClient = httpClient;
            _secretKey = config["Captcha:SecretKey"]
                ?? throw new InvalidOperationException("Captcha SecretKey not configured");
            _logger = logger;

            // ✅ Base address ve timeout ayarla
            _httpClient.BaseAddress = new Uri("https://www.google.com");
            _httpClient.Timeout = TimeSpan.FromSeconds(5);
        }

        public async Task<CaptchaVerificationResultDTO> VerifyAsync(string token)
        {
            try
            {
                // ✅ URL encode edilmiş content
                var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "secret", _secretKey },
                { "response", token }
            });

                // ✅ Timeout korumalı request
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

                var response = await _httpClient.PostAsync(
                    "/recaptcha/api/siteverify",
                    content,
                    cts.Token
                );

                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<CaptchaResponseDTO>(
                    cancellationToken: cts.Token
                );

                return new CaptchaVerificationResultDTO
                {
                    Success = result?.Success ?? false,
                    ErrorCodes = result?.ErrorCodes ?? new List<string>()
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogError("Captcha verification timeout after 5 seconds");
                return new CaptchaVerificationResultDTO
                {
                    Success = false,
                    ErrorCodes = new List<string> { "timeout" }
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Captcha verification HTTP error");
                return new CaptchaVerificationResultDTO
                {
                    Success = false,
                    ErrorCodes = new List<string> { "network-error" }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Captcha verification unexpected error");
                return new CaptchaVerificationResultDTO
                {
                    Success = false,
                    ErrorCodes = new List<string> { "unknown-error" }
                };
            }
        }
    }
}
