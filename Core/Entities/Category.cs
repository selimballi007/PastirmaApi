using System.ComponentModel.DataAnnotations;

namespace PastirmaApi.Core.Entities
{
    public class Category : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(10)]
        public string Icon { get; set; } = "📦";

        public int DisplayOrder { get; set; }

        // Navigation
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
