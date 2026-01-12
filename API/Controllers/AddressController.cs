using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PastirmaApi.Application.DTOs.AddressDTOs;
using PastirmaApi.Application.Interfaces.Services;
using PastirmaApi.Core.Exceptions;
using System.Security.Claims;

namespace PastirmaApi.API.Controllers
{
    [ApiController]
    [Route("api/address")]
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _addressService;

        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<List<AddressDTO>>> GetUserAddresses()
        {
            var userId = GetCurrentUserId();
            var addresses = await _addressService.GetUserAddressesAsync(userId);
            return Ok(addresses);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<AddressDTO>> GetAddress(int id)
        {
            var userId = GetCurrentUserId();
            var address = await _addressService.GetAddressByIdAsync(userId, id);
            return Ok(address);
        }

        [HttpGet("default")]
        [Authorize]
        public async Task<ActionResult<AddressDTO>> GetDefaultAddress()
        {
            var userId = GetCurrentUserId();
            var address = await _addressService.GetDefaultAddressAsync(userId);

            if (address == null)
                return NotFound(new { message = "Varsayılan adres bulunamadı." });

            return Ok(address);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<AddressDTO>> CreateAddress([FromBody] CreateAddressDTO dto)
        {
            var userId = GetCurrentUserId();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var address = await _addressService.CreateAddressAsync(userId, dto);
            return CreatedAtAction(nameof(GetAddress), new { id = address.Id }, address);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<AddressDTO>> UpdateAddress(int id, [FromBody] UpdateAddressDTO dto)
        {
            var userId = GetCurrentUserId();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var address = await _addressService.UpdateAddressAsync(userId, id, dto);
            return Ok(address);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteAddress(int id)
        {
            var userId = GetCurrentUserId();
            await _addressService.DeleteAddressAsync(userId, id);
            return Ok(new { message = "Adres başarıyla silindi." });
        }

        [HttpPut("{id}/set-default")]
        [Authorize]
        public async Task<ActionResult<AddressDTO>> SetDefaultAddress(int id)
        {
            var userId = GetCurrentUserId();
            var address = await _addressService.SetDefaultAddressAsync(userId, id);
            return Ok(address);
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new BusinessException("Geçersiz kullanıcı.");
            }
            return userId;
        }
    }
}
