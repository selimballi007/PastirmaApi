using Microsoft.Extensions.Caching.Memory;
using PastirmaApi.Infrastructure.GoogleCaptcha;
using System.Text;
using System.Text.Json;

namespace PastirmaApi.API.Middlewares
{
    public class CaptchaMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ICaptchaService _captchaService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CaptchaMiddleware> _logger;

        // ✅ HashSet ile O(1) lookup
        private static readonly HashSet<string> ProtectedPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/user/login",
        "/api/user/register",
        "/api/user/verify-email",
        "/api/user/reset-password",
        "/api/user/forgot-password",
        "/api/user/resend-verification-byt",
        "/api/user/resend-verification-bye"
    };

        public CaptchaMiddleware(
            RequestDelegate next,
            ICaptchaService captchaService,
            IMemoryCache cache,
            ILogger<CaptchaMiddleware> logger)
        {
            _next = next;
            _captchaService = captchaService;
            _cache = cache;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // ✅ OPTIONS (preflight) request'leri atla
            if (context.Request.Method == "OPTIONS")
            {
                await _next(context);
                return;
            }

            // ✅ Early return: Korumasız endpoint'lerde 0ms overhead
            if (!IsProtectedEndpoint(context.Request.Path))
            {
                await _next(context);
                return;
            }

            // ✅ Model binding'den önce body'yi okuma (doğru yol)
            context.Request.EnableBuffering();

            string? captchaToken = null;

            try
            {
                // ✅ Body'den captcha token çıkar
                using var reader = new StreamReader(
                    context.Request.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    bufferSize: 1024,
                    leaveOpen: true
                );

                var body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0; // Reset for controller

                if (!string.IsNullOrEmpty(body))
                {
                    var json = JsonDocument.Parse(body);
                    if (json.RootElement.TryGetProperty("captchaToken", out var element))
                    {
                        captchaToken = element.GetString();
                    }
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Invalid JSON in request body");
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { errors = new[] { "Geçersiz request formatı" } });
                return;
            }

            // ✅ Token kontrolü
            if (string.IsNullOrWhiteSpace(captchaToken))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { errors = new[] { "Captcha token gerekli" } });
                return;
            }

            // ✅ CACHE CHECK (Çok önemli!)
            var cacheKey = $"captcha:verified:{captchaToken}";

            if (_cache.TryGetValue<bool>(cacheKey, out var isCached))
            {
                if (!isCached)
                {
                    // Cache'de var ve geçersiz
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsJsonAsync(new { errors = new[] { "Captcha doğrulaması başarısız" } });
                    return;
                }

                // Cache'de var ve geçerli - direkt geç
                await _next(context);
                return;
            }

            // ✅ Google'a doğrulat (sadece cache miss'te)
            var verificationResult = await _captchaService.VerifyAsync(captchaToken);

            // ✅ Sonucu cache'e kaydet (1 dakika - token tekrar kullanılamaz)
            _cache.Set(
                cacheKey,
                verificationResult.Success,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1),
                    Priority = CacheItemPriority.High,
                    Size=1
                }
            );

            if (!verificationResult.Success)
            {
                _logger.LogWarning(
                    "Captcha verification failed. Errors: {Errors}",
                    string.Join(", ", verificationResult.ErrorCodes ?? new List<string>())
                );

                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { errors = new[] { "Captcha doğrulaması başarısız" } });
                return;
            }

            await _next(context);
        }

        private static bool IsProtectedEndpoint(PathString path)
        {
            // ✅ O(1) HashSet lookup - 7 string comparison yerine
            return ProtectedPaths.Contains(path.Value ?? string.Empty);
        }
    }
}
