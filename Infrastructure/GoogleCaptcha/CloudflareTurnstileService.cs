using PastirmaApi.Application.DTOs.CaptchaDTOs;

namespace PastirmaApi.Infrastructure.GoogleCaptcha
{
    /// <summary>
    /// Cloudflare Turnstile verification service
    /// </summary>
    public class CloudflareTurnstileService : ICaptchaService
    {
        private readonly HttpClient _httpClient;
        private readonly string _secretKey;
        private readonly ILogger<CloudflareTurnstileService> _logger;

        public CloudflareTurnstileService(
            HttpClient httpClient,
            IConfiguration config,
            ILogger<CloudflareTurnstileService> logger)
        {
            _httpClient = httpClient;
            _secretKey = config["Turnstile:SecretKey"]
                ?? throw new InvalidOperationException("Turnstile SecretKey not configured");
            _logger = logger;

            // ✅ Cloudflare Turnstile endpoint
            _httpClient.BaseAddress = new Uri("https://challenges.cloudflare.com");
            _httpClient.Timeout = TimeSpan.FromSeconds(5);
        }

        public async Task<CaptchaVerificationResultDTO> VerifyAsync(string token)
        {
            try
            {
                // ✅ URL encoded content for Turnstile verification
                var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "secret", _secretKey },
                    { "response", token }
                });

                // ✅ Timeout protection
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

                var response = await _httpClient.PostAsync(
                    "/turnstile/v0/siteverify",
                    content,
                    cts.Token
                );

                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<CaptchaResponseDTO>(
                    cancellationToken: cts.Token
                );

                if (result?.Success == true)
                {
                    _logger.LogInformation("Turnstile verification successful");
                }
                else
                {
                    _logger.LogWarning("Turnstile verification failed: {ErrorCodes}",
                        string.Join(", ", result?.ErrorCodes ?? new List<string>()));
                }

                return new CaptchaVerificationResultDTO
                {
                    Success = result?.Success ?? false,
                    ErrorCodes = result?.ErrorCodes ?? new List<string>()
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogError("Turnstile verification timeout after 5 seconds");
                return new CaptchaVerificationResultDTO
                {
                    Success = false,
                    ErrorCodes = new List<string> { "timeout" }
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Turnstile verification HTTP error");
                return new CaptchaVerificationResultDTO
                {
                    Success = false,
                    ErrorCodes = new List<string> { "network-error" }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Turnstile verification unexpected error");
                return new CaptchaVerificationResultDTO
                {
                    Success = false,
                    ErrorCodes = new List<string> { "unknown-error" }
                };
            }
        }
    }
}
