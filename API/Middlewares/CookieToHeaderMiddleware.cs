using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace PastirmaApi.API.Middlewares
{
    /// <summary>
    /// Middleware that reads JWT token from HttpOnly cookie and adds it to Authorization header
    /// This allows seamless integration with existing JWT authentication while using secure cookies
    /// </summary>
    public class CookieToHeaderMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CookieToHeaderMiddleware> _logger;

        public CookieToHeaderMiddleware(RequestDelegate next, ILogger<CookieToHeaderMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only process if Authorization header is NOT already present
            if (!context.Request.Headers.ContainsKey("Authorization"))
            {
                // Try to get accessToken from cookie
                if (context.Request.Cookies.TryGetValue("accessToken", out var token))
                {
                    // Add to Authorization header for JWT middleware to process
                    context.Request.Headers["Authorization"] = $"Bearer {token}";

                    _logger.LogDebug("Added Authorization header from accessToken cookie");
                }
            }
            else
            {
                _logger.LogDebug("Authorization header already present, skipping cookie check");
            }

            await _next(context);
        }
    }
}
