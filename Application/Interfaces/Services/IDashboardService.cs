using PastirmaApi.Application.DTOs.DashboardDTOs;
using PastirmaApi.Application.DTOs.OrderDTOs;

namespace PastirmaApi.Application.Interfaces.Services
{
    public interface IDashboardService
    {
        Task<DashboardResponse> GetDashboardDataAsync();
        Task<DashboardStats> GetStatsAsync();
        Task<List<OrderDTO>> GetRecentOrdersAsync(int limit);
        Task<List<SalesDataPoint>> GetSalesDataAsync(string period);
        Task<QuickStats> GetQuickStatsAsync();
        Task<int> GetNotificationCountAsync();
        Task<object> GetLowStockAlertsAsync();
    }
}
