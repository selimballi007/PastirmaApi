using Microsoft.AspNetCore.Mvc;
using PastirmaApi.Application.DTOs.UserDTOs;
using PastirmaApi.Application.Interfaces.Services;
using PastirmaApi.Core.Exceptions;
using System.Security.Claims;

namespace PastirmaApi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController:ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IWebHostEnvironment _env;

        public UserController( IUserService userService, IWebHostEnvironment env)
        {
            _userService = userService;
            _env = env;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDTO dto) 
        {
            await _userService.RegisterUserAsync(dto);
            return Ok(new { message = "Kullanıcı başarıyla oluşturuldu. Lütfen email hesabınıza gelen link ile doğrulama işlemini yapınız." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDTO dto)
        {
            var result = await _userService.LoginUserAsync(dto);

            RefreshTokenCookieSettings(result.refreshTokenExpiry, result.refreshToken);

            //Return access token with JSON response (frontend stores)
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
            var refreshToken = Request.Cookies["refreshToken"]!;
            if(refreshToken == null)
                throw new AuthException("Tekrar Giriş yapınız");
            var authHeader = Request.Headers["Authorization"].ToString();
            string? accessToken = null;

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                accessToken = authHeader["Bearer ".Length..].Trim();
            }

            var result = await _userService.RefreshAccessTokenAsync(refreshToken, accessToken);

            RefreshTokenCookieSettings(result.refreshTokenExpiry, result.refreshToken);

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
        private void RefreshTokenCookieSettings(DateTime? refreshTokenExpiry, string refreshToken)
        {
            
            // Put the refresh token in the cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true, //JS inaccessible → secure
                Expires = refreshTokenExpiry,
                Path = "/"                
            };
            if (_env.IsDevelopment())
            {
                cookieOptions.Secure = false;
                cookieOptions.SameSite= SameSiteMode.None;
            }
            else
            {
                cookieOptions.Secure = true;// Only works on HTTPS
                cookieOptions.SameSite = SameSiteMode.Strict;// Protects against CSRF
            }

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }

    }
}
