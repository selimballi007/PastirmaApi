using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PastirmaApi.Application.DTOs.ProductDTOs;
using PastirmaApi.Application.Interfaces.Services;
using PastirmaApi.Core.Entities;
using PastirmaApi.Core.Exceptions;
using PastirmaApi.Infrastructure.Data;
using System.Security.Claims;

namespace PastirmaApi.Application.Services
{
    public class FavoriteService : IFavoriteService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FavoriteService> _logger;

        public FavoriteService(ApplicationDbContext context, ILogger<FavoriteService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> AddToFavoritesAsync(int userId, int productId)
        {
            try
            {
                // Ürün var mı?
                var productExists = await _context.Products.AnyAsync(p => p.Id == productId);
                if (!productExists)
                {
                    throw new NotFoundException("Ürün bulunamadı.");
                }

                // Zaten favorilerde mi?
                var exists = await _context.Favorites
                    .AnyAsync(f => f.UserId == userId && f.ProductId == productId);

                if (exists)
                {
                    return true; // Zaten var
                }

                var favorite = new Favorite
                {
                    UserId = userId,
                    ProductId = productId
                };

                _context.Favorites.Add(favorite);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Product added to favorites: UserId={UserId}, ProductId={ProductId}",
                    userId, productId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding to favorites");
                throw;
            }
        }

        public async Task<bool> RemoveFromFavoritesAsync(int userId, int productId)
        {
            try
            {
                var favorite = await _context.Favorites
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);

                if (favorite == null)
                {
                    return false;
                }

                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Product removed from favorites: UserId={UserId}, ProductId={ProductId}",
                    userId, productId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing from favorites");
                throw;
            }
        }

        public async Task<bool> IsFavoriteAsync(int userId, int productId)
        {
            return await _context.Favorites
                .AnyAsync(f => f.UserId == userId && f.ProductId == productId);
        }

        public async Task<List<ProductDTO>> GetUserFavoritesAsync(int userId)
        {
            var favorites = await _context.Favorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Product)
                    .ThenInclude(p => p.Category)
                .OrderByDescending(f => f.CreatedDate)
                .Select(f => new ProductDTO
                {
                    Id = f.Product.Id,
                    Name = f.Product.Name,
                    Description = f.Product.Description,
                    Price = f.Product.Price,
                    OldPrice = f.Product.OldPrice,
                    ImageUrl = f.Product.ImageUrl,
                    CategoryId = f.Product.CategoryId,
                    CategoryName = f.Product.Category.Name,
                    IsCampaign = f.Product.IsCampaign,
                    IsBestSeller = f.Product.IsBestseller,
                    IsNew = f.Product.IsNew,
                    IsActive = f.Product.IsActive,
                    CreatedAt = f.Product.CreatedDate
                })
                .ToListAsync();

            return favorites;
        }

        public async Task<int> GetFavoriteCountAsync(int userId)
        {
            return await _context.Favorites
                .CountAsync(f => f.UserId == userId);
        }
    }

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

        // POST: api/favorites/{productId}
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
