using Microsoft.AspNetCore.Mvc;
using PastirmaApi.Models;
using PastirmaApi.Services;

namespace PastirmaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController:ControllerBase
    {
        private readonly UserService _service;

        public UserController(UserService service)
        {
            _service = service;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDTO dto) {

            if (await _service.UserExistAsync(dto.Email))
                return BadRequest("Kullanıcı adı mevcut");

            var user = await _service.RegisterUserAsync(dto);
            return Ok(new { user.Id, user.Email, user.Role });
        }

        public class RegisterUserDTO
        {
            public string Email { get; set; } = null!;
            public string PasswordHash { get; set; } = null!;
            public UserRole Role { get; set; } = UserRole.Customer;
        }
    }
}
