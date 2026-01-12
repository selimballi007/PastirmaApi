namespace PastirmaApi.Application.DTOs.DashboardDTOs
{
    public class DashboardStats
    {
        public decimal TotalSales { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalProducts { get; set; }
        public double SalesChange { get; set; }
        public double OrdersChange { get; set; }
        public double CustomersChange { get; set; }
        public double ProductsChange { get; set; }
    }
}
