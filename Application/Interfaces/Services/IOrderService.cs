using PastirmaApi.Application.DTOs.DashboardDTOs;
using PastirmaApi.Application.DTOs.OrderDTOs;
using PastirmaApi.Core.Entities;

namespace PastirmaApi.Application.Interfaces.Services
{
    public interface IOrderService
    {
        Task<PaginatedResponse<OrderDTO>> GetOrdersAsync(string userId, int page, int pageSize, string? status);
        Task<OrderDTO?> GetOrderDetailsAsync(string userId, string orderId);
        Task<bool> UpdateOrderStatusAsync(string userId, string orderId, OrderStatus status);
    }
}
