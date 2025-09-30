using PastirmaApi.Core.Exceptions;

namespace PastirmaApi.API.Middlewares
{
    /// <summary>
    /// Frontendde requestten sonra catch e düşen hataları aynı formatta alabilmek için tasarlandı.
    /// Object.values(err.response.data.errors).flat() as string[]; -> Bu ifade ile hataları alıyoruz.
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (BaseException ex)
            {
                context.Response.StatusCode = ex.StatusCode;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { errors = new[] { ex.Message} });                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Beklenmeyen bir hata oluştu");
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new {errors = new[] { "Beklenmeyen bir hata oluştu"} });
            }
        }
    }
}
