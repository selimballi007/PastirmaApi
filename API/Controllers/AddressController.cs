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
        private readonly ILogger<AddressController> _logger;

        public AddressController(IAddressService addressService, ILogger<AddressController> logger)
        {
            _addressService = addressService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<List<AddressDTO>>> GetUserAddresses()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "Kullanıcı kimliği bulunamadı." });

                var addresses = await _addressService.GetUserAddressesAsync(int.Parse(userId));
                return Ok(addresses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user addresses");
                return StatusCode(500, new { message = "Adresler getirilirken bir hata oluştu." });
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<AddressDTO>> GetAddress(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "Kullanıcı kimliği bulunamadı." });

                var address = await _addressService.GetAddressByIdAsync(int.Parse(userId), id);
                return Ok(address);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting address {AddressId}", id);
                return StatusCode(500, new { message = "Adres getirilirken bir hata oluştu." });
            }
        }

        [HttpGet("default")]
        [Authorize]
        public async Task<ActionResult<AddressDTO>> GetDefaultAddress()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "Kullanıcı kimliği bulunamadı." });

                var address = await _addressService.GetDefaultAddressAsync(int.Parse(userId));

                if (address == null)
                    return NotFound(new { message = "Varsayılan adres bulunamadı." });

                return Ok(address);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting default address");
                return StatusCode(500, new { message = "Varsayılan adres getirilirken bir hata oluştu." });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<AddressDTO>> CreateAddress([FromBody] CreateAddressDTO dto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "Kullanıcı kimliği bulunamadı." });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var address = await _addressService.CreateAddressAsync(int.Parse(userId), dto);
                return CreatedAtAction(nameof(GetAddress), new { id = address.Id }, address);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating address");
                return StatusCode(500, new { message = "Adres oluşturulurken bir hata oluştu." });
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<AddressDTO>> UpdateAddress(int id, [FromBody] UpdateAddressDTO dto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "Kullanıcı kimliği bulunamadı." });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var address = await _addressService.UpdateAddressAsync(int.Parse(userId), id, dto);
                return Ok(address);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating address {AddressId}", id);
                return StatusCode(500, new { message = "Adres güncellenirken bir hata oluştu." });
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteAddress(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "Kullanıcı kimliği bulunamadı." });

                await _addressService.DeleteAddressAsync(int.Parse(userId), id);
                return Ok(new { message = "Adres başarıyla silindi." });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting address {AddressId}", id);
                return StatusCode(500, new { message = "Adres silinirken bir hata oluştu." });
            }
        }

        [HttpPut("{id}/set-default")]
        [Authorize]
        public async Task<ActionResult<AddressDTO>> SetDefaultAddress(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "Kullanıcı kimliği bulunamadı." });

                var address = await _addressService.SetDefaultAddressAsync(int.Parse(userId), id);
                return Ok(address);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting default address {AddressId}", id);
                return StatusCode(500, new { message = "Varsayılan adres ayarlanırken bir hata oluştu." });
            }
        }
    }
}
