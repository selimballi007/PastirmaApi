using PastirmaApi.Core.Entities;

namespace PastirmaApi.Application.DTOs.ProductDTOs
{
    public class ProductDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public bool IsCampaign { get; set; }
        public bool IsBestSeller { get; set; }
        public bool IsNew { get; set; }
        public int Stock { get; set; }
        public int? CategoryId { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public bool IsSpecialOffer { get; set; }
        public int CampaignOrder { get; set; }
        public int BestSellerOrder { get; set; }
        public int Rating { get; set; } = 0;
        public int ReviewCount { get; set; } = 0;
        public int SalesCount { get; set; } = 0;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<ProductImage>? Images { get; set; } = new List<ProductImage>();
    }
}
