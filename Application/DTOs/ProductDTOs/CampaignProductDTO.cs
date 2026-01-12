namespace PastirmaApi.Application.DTOs.ProductDTOs
{
    public class CampaignProductDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int Stock { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public double? Rating { get; set; }
        public int? ReviewCount { get; set; }
        public bool IsCampaign { get; set; }
        public bool IsActive { get; set; }
        public int? CampaignOrder { get; set; }    
    }
}
