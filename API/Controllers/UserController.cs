using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PastirmaApi.Application.DTOs.UserDTOs;
using PastirmaApi.Application.Interfaces.Services;
using PastirmaApi.Core.Exceptions;
using System.Diagnostics;
using System.Security.Claims;

namespace PastirmaApi.API.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController:ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;

        public UserController(IUserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _configuration = configuration;
        }

        [HttpPost("register")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDTO dto) 
        {
            await _userService.RegisterUserAsync(dto);
            return Ok(new { message = "Kullanıcı başarıyla oluşturuldu. Lütfen email hesabınıza gelen link ile doğrulama işlemini yapınız." });
        }

        [HttpPost("login")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO dto)
        {
            var result = await _userService.LoginUserAsync(dto);

            // Set both access and refresh tokens as HttpOnly cookies
            AccessTokenCookieSettings(result.accessToken);
            RefreshTokenCookieSettings(result.refreshTokenExpiry, result.refreshToken);           

            return Ok(new {
                accessToken = result.accessToken, // Still return for backward compatibility
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
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> ResendVerificationByToken([FromBody] ResendVerificationByTokenDTO dto)
        {
            await _userService.ResendVerificationByTokenAsync(dto.Token);
            return Ok(new { message = "Yeni doğrulama e-postası gönderildi." });
        }

        [HttpPost("resend-verification-bye")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> ResendVerificationByEmail([FromBody] ResendVerificationByEmailDTO dto)
        {
            await _userService.ResendVerificationByEmailAsync(dto.Email);
            return Ok(new { message = "Yeni doğrulama e-postası gönderildi." });
        }

        [HttpPost("forgot-password")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO dto)
        {
            await _userService.ForgotPasswordAsync(dto.Email);
            return Ok(new { message = "Eğer kayıtlı bir hesabınız varsa, şifre sıfırlama linki gönderildi." });
        }

        [HttpPost("reset-password")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO dto)
        {
            await _userService.ResetPasswordAsync(dto);
            return Ok(new { message = "Şifreniz başarıyla güncellendi." });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {            
            foreach (var cookie in Request.Cookies)
            {
                var value = cookie.Value.Length > 50 ? cookie.Value.Substring(0, 50) + "..." : cookie.Value;
            }

            // ✅ Read both tokens from cookies (cookie-based auth)
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                throw new BusinessException("Tekrar Giriş yapınız");
            }

            var accessToken = Request.Cookies["accessToken"];

            if (string.IsNullOrEmpty(accessToken))
            {
                throw new BusinessException("Tekrar Giriş yapınız");
            }

            var result = await _userService.RefreshAccessTokenAsync(refreshToken, accessToken);

            // Set both access and refresh tokens as HttpOnly cookies
            AccessTokenCookieSettings(result.accessToken);
            RefreshTokenCookieSettings(result.refreshTokenExpiry, result.refreshToken);
            
            return Ok(new
            {
                user = new LoginResponseDTO(result.id, result.userName, result.email, result.role, result.lastLoginAt)
            });              
        }

        [HttpPost("logout")]
        [Authorize] // ✅ Require authentication - only logged in users can logout
        public async Task<IActionResult> LogoutUserAsync()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // ✅ Clear RefreshToken from User table in database
            await _userService.LogoutAsync(int.Parse(userId));

            // Clear both access and refresh token cookies
            Response.Cookies.Delete("accessToken");
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

        [HttpGet("customers")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllCustomers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var result = await _userService.GetAllCustomersAsync(page, pageSize);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Müşteriler getirilirken bir hata oluştu." });
            }
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var profile = await _userService.GetUserProfileAsync(int.Parse(userId));
                return Ok(profile);
            }
            catch (BusinessException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Profil bilgileri getirilirken bir hata oluştu." });
            }
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDTO dto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var profile = await _userService.UpdateUserProfileAsync(int.Parse(userId), dto);
                return Ok(new { message = "Profil başarıyla güncellendi", profile });
            }
            catch (BusinessException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Profil güncellenirken bir hata oluştu." });
            }
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO dto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                await _userService.ChangePasswordAsync(int.Parse(userId), dto);
                return Ok(new { message = "Şifre başarıyla değiştirildi" });
            }
            catch (BusinessException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Şifre değiştirilirken bir hata oluştu." });
            }
        }

        private void AccessTokenCookieSettings(string accessToken)
        {
            var env = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();

            // Read access token expiration from configuration (single source of truth)
            var accessTokenExpiresMinutes = _configuration.GetValue<int>("Jwt:AccessTokenExpiresMinutes");

            // Put the access token in the cookie (matches JWT expiry from configuration)
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true, // JS inaccessible → secure
                Expires = DateTimeOffset.UtcNow.AddMinutes(accessTokenExpiresMinutes), // Match JWT expiry
                Path = "/"
            };
            if (env.IsDevelopment())
            {
                // Development (localhost): Use Lax for localhost testing
                // Note: SameSite=None requires Secure=true (HTTPS), but we're using HTTP in dev
                // So we use Lax which works for same-site (localhost) and is more compatible
                cookieOptions.Secure = false;
                cookieOptions.SameSite = SameSiteMode.Lax;
            }
            else
            {
                // Production & Staging (test): Use strict secure settings
                // Requires same-domain architecture (Option A) for cookies to work
                cookieOptions.Secure = true; // HTTPS only
                cookieOptions.SameSite = SameSiteMode.Strict; // Maximum CSRF protection
            }

            Response.Cookies.Append("accessToken", accessToken, cookieOptions);
        }

        private void RefreshTokenCookieSettings(DateTime? refreshTokenExpiry, string refreshToken)
        {
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
                // Development (localhost): Use Lax for localhost testing
                // Note: SameSite=None requires Secure=true (HTTPS), but we're using HTTP in dev
                // So we use Lax which works for same-site (localhost) and is more compatible
                cookieOptions.Secure = false;
                cookieOptions.SameSite = SameSiteMode.Lax;
            }
            else
            {
                // Production & Staging (test): Use strict secure settings
                // Requires same-domain architecture (Option A) for cookies to work
                cookieOptions.Secure = true; // HTTPS only
                cookieOptions.SameSite = SameSiteMode.Strict; // Maximum CSRF protection
            }

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }

    }
}
