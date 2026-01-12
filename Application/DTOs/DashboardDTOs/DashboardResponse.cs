using PastirmaApi.Application.DTOs.OrderDTOs;

namespace PastirmaApi.Application.DTOs.DashboardDTOs
{
    public class DashboardResponse
    {
        public DashboardStats? Stats { get; set; }
        public List<OrderDTO>? RecentOrders { get; set; }
        public List<SalesDataPoint>? SalesData { get; set; }
        public QuickStats? QuickStats { get; set; }
    }
}
