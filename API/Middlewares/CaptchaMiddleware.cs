namespace PastirmaApi.API.Middlewares
{
    public class CaptchaMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _config;
        private readonly ILogger<CaptchaMiddleware> _logger;

        public CaptchaMiddleware(RequestDelegate next, IConfiguration config, ILogger<CaptchaMiddleware> logger)
        {
            _next = next;
            _config = config;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Sadece belirli endpoint’lerde çalıştır
            if (context.Request.Path.StartsWithSegments("/api/user/login") ||
                context.Request.Path.StartsWithSegments("/api/user/register") ||
                context.Request.Path.StartsWithSegments("/api/user/verify-email") ||
                context.Request.Path.StartsWithSegments("/api/user/reset-password") ||
                context.Request.Path.StartsWithSegments("/api/user/forgot-password") ||
                context.Request.Path.StartsWithSegments("/api/user/resend-verification-byt") ||
                context.Request.Path.StartsWithSegments("/api/user/resend-verification-bye"))
            {
                try
                {
                    // Body'den al
                    context.Request.EnableBuffering();
                    using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
                    var body = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0;

                    if (!string.IsNullOrEmpty(body))
                    {
                        var json = System.Text.Json.JsonDocument.Parse(body);
                        if (json.RootElement.TryGetProperty("captchaToken", out var captchaElement))
                        {
                            var captchaToken = captchaElement.GetString();

                            if (string.IsNullOrEmpty(captchaToken))
                            {
                                context.Response.StatusCode = 400;
                                await context.Response.WriteAsJsonAsync(new { message = "Captcha token yok" });
                                return;
                            }

                            // Google’a doğrulat
                            using var httpClient = new HttpClient();
                            var secret = _config["Captcha:SecretKey"];
                            var verifyUrl =
                                $"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={captchaToken}";
                            var res = await httpClient.PostAsync(verifyUrl, null);
                            var captchaResult = await res.Content.ReadFromJsonAsync<CaptchaResponse>();

                            if (captchaResult == null || !captchaResult.Success)
                            {
                                context.Response.StatusCode = 400;
                                await context.Response.WriteAsJsonAsync(new { message = "Captcha doğrulaması başarısız" });
                                return;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Captcha doğrulama sırasında hata oluştu");
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsJsonAsync(new { message = "Captcha kontrolünde hata oluştu" });
                    return;
                }
            }

            // Doğrulama başarılı → request devam etsin
            await _next(context);
        }
    }

    public class CaptchaResponse
    {
        public bool Success { get; set; }
        public DateTime Challenge_ts { get; set; }
        public string Hostname { get; set; }
        public List<string> ErrorCodes { get; set; } = new();
    }
}
