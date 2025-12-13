using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PastirmaApi.Application.DTOs.ProductDTOs;
using PastirmaApi.Application.Interfaces.Services;
using PastirmaApi.Core.Exceptions;
using System.Security.Claims;

namespace PastirmaApi.API.Controllers
{
    // Controllers/FavoritesController.cs
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FavoritesController : ControllerBase
    {
        private readonly IFavoriteService _favoriteService;
        private readonly ILogger<FavoritesController> _logger;

        public FavoritesController(IFavoriteService favoriteService, ILogger<FavoritesController> logger)
        {
            _favoriteService = favoriteService;
            _logger = logger;
        }

        // POST: api/favorite/{productId}
        [HttpPost("{productId}")]
        public async Task<ActionResult> AddToFavorites(int productId)
        {
            try
            {
                var userId = GetUserId();
                await _favoriteService.AddToFavoritesAsync(userId, productId);
                return Ok(new { message = "Ürün favorilere eklendi." });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding to favorites");
                return StatusCode(500, new { message = "Favorilere eklenirken bir hata oluştu." });
            }
        }

        // DELETE: api/favorites/{productId}
        [HttpDelete("{productId}")]
        public async Task<ActionResult> RemoveFromFavorites(int productId)
        {
            try
            {
                var userId = GetUserId();
                var result = await _favoriteService.RemoveFromFavoritesAsync(userId, productId);

                if (result)
                {
                    return Ok(new { message = "Ürün favorilerden çıkarıldı." });
                }

                return NotFound(new { message = "Favori bulunamadı." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing from favorites");
                return StatusCode(500, new { message = "Favorilerden çıkarılırken bir hata oluştu." });
            }
        }

        // GET: api/favorites/check/{productId}
        [HttpGet("check/{productId}")]
        public async Task<ActionResult<bool>> IsFavorite(int productId)
        {
            try
            {
                var userId = GetUserId();
                var isFavorite = await _favoriteService.IsFavoriteAsync(userId, productId);
                return Ok(new { isFavorite });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking favorite");
                return StatusCode(500, new { message = "Kontrol sırasında bir hata oluştu." });
            }
        }

        // GET: api/favorites
        [HttpGet]
        public async Task<ActionResult<List<ProductDTO>>> GetUserFavorites()
        {
            try
            {
                var userId = GetUserId();
                var favorites = await _favoriteService.GetUserFavoritesAsync(userId);
                return Ok(favorites);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting favorites");
                return StatusCode(500, new { message = "Favoriler getirilirken bir hata oluştu." });
            }
        }

        // GET: api/favorites/count
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetFavoriteCount()
        {
            try
            {
                var userId = GetUserId();
                var count = await _favoriteService.GetFavoriteCountAsync(userId);
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting favorite count");
                return StatusCode(500, new { message = "Sayı alınırken bir hata oluştu." });
            }
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("Geçersiz kullanıcı.");
            }
            return userId;
        }
    }
}
