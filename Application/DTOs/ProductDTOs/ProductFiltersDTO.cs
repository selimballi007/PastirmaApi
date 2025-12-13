namespace PastirmaApi.Application.DTOs.ProductDTOs
{
    public class ProductFiltersDTO
    {
        public int CategoryId { get; set; }
        public bool? IsActive { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Search { get; set; }
        public bool IsBestSeller { get; set; }
        public bool IsCampaign { get; set; }
        public int? Limit { get; set; }
    }
}
