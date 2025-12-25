using System.ComponentModel.DataAnnotations;

namespace PastirmaApi.Core.Entities
{
    public class BlogCategory : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Icon { get; set; } = "📝";

        public int DisplayOrder { get; set; }

        // Navigation properties
        public ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
    }
}
