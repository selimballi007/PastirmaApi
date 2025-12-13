namespace PastirmaApi.Application.DTOs.DashboardDTOs
{
    public class SalesDataPoint
    {
        public string Date { get; set; } = String.Empty;// ISO format date string
        public decimal Sales { get; set; }
        public int Orders { get; set; }
    }
}
