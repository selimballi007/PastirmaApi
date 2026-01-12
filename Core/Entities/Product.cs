using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PastirmaApi.Core.Entities
{
    public class Product:BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        [Required]
        public int Stock { get; set; }
        [Required]
        public int CategoryId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsBestseller { get; set; } = false;
        public int BestsellerOrder { get; set; }
        public bool IsCampaign { get; set; } = false;
        public int CampaignOrder { get; set; }
        public bool IsNew { get; set; } = false;
        public bool IsSpecialOffer { get; set; } = false;
        [Column(TypeName = "decimal(18,2)")]
        public decimal? OldPrice { get; set; }

        // Navigation properties
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public Category Category { get; set; } = null!;
    }
}
