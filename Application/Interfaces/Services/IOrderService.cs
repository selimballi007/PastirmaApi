using PastirmaApi.Application.DTOs.DashboardDTOs;
using PastirmaApi.Application.DTOs.OrderDTOs;
using PastirmaApi.Core.Entities;

namespace PastirmaApi.Application.Interfaces.Services
{
    public interface IOrderService
    {
        // Checkout
        Task<OrderDTO> CreateOrderAsync(CreateOrderDTO createOrderDto, int? userId);

        // Order retrieval
        Task<PaginatedResponse<OrderDTO>> GetAllOrdersAsync(int page, int pageSize, string? status);
        Task<PaginatedResponse<OrderDTO>> GetOrdersAsync(string userId, int page, int pageSize, string? status);
        Task<OrderDTO?> GetOrderDetailsAsync(string userId, string orderId);
        Task<OrderDTO?> GetOrderByIdAsync(int orderId);
        Task<OrderDTO?> TrackOrderAsync(string orderNumber, string email);

        // Order management
        Task<bool> UpdateOrderStatusAsync(string userId, string orderId, OrderStatus status);
        Task<bool> UpdateOrderStatusByIdAsync(int orderId, OrderStatus status);
    }
}
