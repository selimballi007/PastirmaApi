using PastirmaApi.Application.DTOs.ProductDTOs;

namespace PastirmaApi.Application.Interfaces.Services
{
    public interface IProductService
    {
        Task<List<ProductDTO>> GetProductsAsync(ProductFiltersDTO filters); 
        Task<ProductDTO?> GetProductByIdAsync(int id, bool includeImages=true);
        Task<ProductDTO> CreateProductAsync(CreateProductRequestDTO request);
        Task<ProductDTO?> UpdateProductAsync(int id, UpdateProductRequestDTO request);
        Task<bool> DeleteProductAsync(int id);
        Task<bool> UpdateProductStatusAsync(int id, bool isActive);
        Task<bool> UpdateProductStockAsync(int id, int stock);
    }
}
