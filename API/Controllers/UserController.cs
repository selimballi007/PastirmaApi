using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PastirmaApi.Application.DTOs.UserDTOs;
using PastirmaApi.Application.Interfaces.Services;
using PastirmaApi.Core.Exceptions;
using System.Diagnostics;
using System.Security.Claims;

namespace PastirmaApi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController:ControllerBase
    {
        private readonly IUserService _userService;

        public UserController( IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDTO dto) 
        {
            await _userService.RegisterUserAsync(dto);
            return Ok(new { message = "Kullanıcı başarıyla oluşturuldu. Lütfen email hesabınıza gelen link ile doğrulama işlemini yapınız." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO dto)
        {
            var controllerStopWatch = Stopwatch.StartNew();
            var result = await _userService.LoginUserAsync(dto);          
            
            RefreshTokenCookieSettings(result.refreshTokenExpiry, result.refreshToken);
            controllerStopWatch.Stop();
            System.Diagnostics.Debug.WriteLine($"ControllerTime : {controllerStopWatch.ElapsedMilliseconds.ToString()}");
            
            return Ok(new { 
                accessToken = result.accessToken,
                user= new LoginResponseDTO(result.id, result.userName, result.email, result.role, result.lastLoginAt )
            });
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDTO dto)
        {
            await _userService.VerifyEmailAsync(dto.Token);
            return Ok(new { message = "Email başarıyla doğrulandı. Artık giriş yapabilirsiniz." });            
        }

        [HttpPost("resend-verification-byt")]
        public async Task<IActionResult> ResendVerificationByToken([FromBody] ResendVerificationByTokenDTO dto)
        {
            await _userService.ResendVerificationByTokenAsync(dto.Token);
            return Ok(new { message = "Yeni doğrulama e-postası gönderildi." });
        }

        [HttpPost("resend-verification-bye")]
        public async Task<IActionResult> ResendVerificationByEmail([FromBody] ResendVerificationByEmailDTO dto)
        {
            await _userService.ResendVerificationByEmailAsync(dto.Email);
            return Ok(new { message = "Yeni doğrulama e-postası gönderildi." });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO dto)
        {
            await _userService.ForgotPasswordAsync(dto.Email);
            return Ok(new { message = "Eğer kayıtlı bir hesabınız varsa, şifre sıfırlama linki gönderildi." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO dto)
        {
            await _userService.ResetPasswordAsync(dto);
            return Ok(new { message = "Şifreniz başarıyla güncellendi." });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var controllerStopWatch = Stopwatch.StartNew();
            var refreshToken = Request.Cookies["refreshToken"]!;
            if(refreshToken == null)
                throw new AuthException("Tekrar Giriş yapınız");           
            var authHeader = Request.Headers["Authorization"].ToString();
            string? accessToken = null;

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                accessToken = authHeader["Bearer ".Length..].Trim();
            }
            if (accessToken == null)
                throw new AuthException("Tekrar Giriş yapınız");

            var result = await _userService.RefreshAccessTokenAsync(refreshToken, accessToken);

            RefreshTokenCookieSettings(result.refreshTokenExpiry, result.refreshToken);
            controllerStopWatch.Stop();
            System.Diagnostics.Debug.WriteLine($"ControllerTime : {controllerStopWatch.ElapsedMilliseconds.ToString()}");
            return Ok(new
            {
                accessToken = result.accessToken,
                user = new LoginResponseDTO(result.id, result.userName, result.email, result.role, result.lastLoginAt)
            });              
        }

        [HttpPost("logout")]
        public async Task<IActionResult> LogoutUserAsync()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            await _userService.LogoutAsync(int.Parse(userId));

            Response.Cookies.Delete("refreshToken");

            return Ok();
        }

        [HttpPost("test")]
        [Authorize]
        public async Task<IActionResult> TestAsync()
        {
            await Task.CompletedTask;
            return Ok(new { message = "Test successful", timestamp = DateTime.UtcNow });
        }

        [HttpGet("test-cors")]
        public IActionResult TestCors()
        {
            return Ok(new
            {
                message = "CORS working",
                cookies = Request.Cookies.Keys,
                refreshToken = Request.Cookies["refreshToken"] ?? "NOT FOUND"
            });
        }
        private void RefreshTokenCookieSettings(DateTime? refreshTokenExpiry, string refreshToken)
        {
            System.Diagnostics.Debug.WriteLine($"[RefreshTokenCookieSettings] Called");
            System.Diagnostics.Debug.WriteLine($"[RefreshTokenCookieSettings] Cookie value: {refreshToken.Substring(0, 20)}...");

            var env = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();

            // Put the refresh token in the cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true, //JS inaccessible → secure
                Expires = refreshTokenExpiry,
                Path = "/"                
            };
            if (env.IsDevelopment())
            {
                cookieOptions.Secure = false;
                cookieOptions.SameSite= SameSiteMode.Lax;
            }
            else
            {
                cookieOptions.Secure = true;// Only works on HTTPS
                cookieOptions.SameSite = SameSiteMode.Strict;// Protects against CSRF
            }

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);

            System.Diagnostics.Debug.WriteLine($"[RefreshTokenCookieSettings] Cookie appended");
            System.Diagnostics.Debug.WriteLine($"[RefreshTokenCookieSettings] Response.Headers[\"Set-Cookie\"]: {Response.Headers["Set-Cookie"]}");

        }

    }
}
