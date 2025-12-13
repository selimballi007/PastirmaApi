using PastirmaApi.Application.DTOs.ProductDTOs;

namespace PastirmaApi.Application.Interfaces.Services
{
    public interface IFavoriteService
    {
        Task<bool> AddToFavoritesAsync(int userId, int productId);
        Task<bool> RemoveFromFavoritesAsync(int userId, int productId);
        Task<bool> IsFavoriteAsync(int userId, int productId);
        Task<List<ProductDTO>> GetUserFavoritesAsync(int userId);
        Task<int> GetFavoriteCountAsync(int userId);
    }
}
