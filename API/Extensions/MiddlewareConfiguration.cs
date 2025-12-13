using PastirmaApi.API.Middlewares;
using PastirmaApi.Infrastructure.GoogleCaptcha;

namespace PastirmaApi.API.Extensions
{
    public static class MiddlewareConfiguration
    {
        public static IServiceCollection AddCaptchaServices(this IServiceCollection services)
        {
            // ✅ HttpClient factory ile - connection pooling otomatik
            services.AddHttpClient<ICaptchaService, GoogleRecaptchaService>()
                .SetHandlerLifetime(TimeSpan.FromMinutes(5)); // Connection pool lifetime

            // ✅ Memory cache
            services.AddMemoryCache(options =>
            {
                options.SizeLimit = 1024; // Max 1024 entry
                options.CompactionPercentage = 0.25;  // %75 dolunca %25'ini temizle
                options.ExpirationScanFrequency = TimeSpan.FromMinutes(5);
            });

            return services;
        }

        public static IApplicationBuilder UseCustomMiddlewares(this IApplicationBuilder app)
        {
            // ✅ SIRA ÖNEMLİ!
            // 1. Exception handling en dışta
            app.UseMiddleware<ErrorHandlingMiddleware>();

            // 2. Captcha middleware
            app.UseMiddleware<CaptchaMiddleware>();

            return app;
        }
    }
}
