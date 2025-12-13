using System.ComponentModel.DataAnnotations;

namespace PastirmaApi.Core.Entities
{
    public class ProductImage : BaseEntity
    {
        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        public int DisplayOrder { get; set; } // Sıralama (1, 2, 3...)

        public bool IsPrimary { get; set; } = false; // Ana görsel mi?
    }
}
