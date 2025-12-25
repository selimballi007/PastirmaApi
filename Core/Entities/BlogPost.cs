using System.ComponentModel.DataAnnotations;

namespace PastirmaApi.Core.Entities
{
    public class BlogPost : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(5000)]
        public string Content { get; set; } = string.Empty; // HTML from TipTap

        [Required]
        [MaxLength(500)]
        public string Excerpt { get; set; } = string.Empty;

        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int AuthorId { get; set; }

        public DateTime? PublishedDate { get; set; } // null = draft

        public bool IsFeatured { get; set; } = false;

        public int ViewCount { get; set; } = 0;

        [MaxLength(10)]
        public string ReadTime { get; set; } = "1 min"; // Auto-calculated

        // Navigation properties
        public BlogCategory Category { get; set; } = null!;
        public User Author { get; set; } = null!;
    }
}
