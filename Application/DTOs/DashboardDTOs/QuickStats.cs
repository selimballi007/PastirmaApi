namespace PastirmaApi.Application.DTOs.DashboardDTOs
{
    public class QuickStats
    {
        public double ConversionRate { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int ActiveUsers { get; set; }
        public int LowStockProducts { get; set; }
    }
}
