using Microsoft.AspNetCore.Mvc;
using PastirmaApi.Application.DTOs.UserDTOs;
using PastirmaApi.Application.Interfaces.Services;

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
        public async Task<IActionResult> Login([FromBody] LoginUserDTO dto)
        {
            var result = await _userService.LoginUserAsync(dto);            
            return Ok(result);            
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] ResendVerificationDTO dto)
        {
            await _userService.VerifyEmailAsync(dto.Token);
            return Ok(new { message = "Email başarıyla doğrulandı. Artık giriş yapabilirsiniz." });            
        }

        [HttpPost("resend-verification-byt")]
        public async Task<IActionResult> ResendVerificationByToken([FromBody] ResendVerificationDTO dto)
        {
            await _userService.ResendVerificationByTokenAsync(dto.Token);
            return Ok(new { message = "Yeni doğrulama e-postası gönderildi." });
        }

        [HttpPost("resend-verification-bye")]
        public async Task<IActionResult> ResendVerificationByEmail([FromBody] ResendVerificationDTO dto)
        {
            await _userService.ResendVerificationByEmailAsync(dto.Email);
            return Ok(new { message = "Yeni doğrulama e-postası gönderildi." });
        }

    }
}
