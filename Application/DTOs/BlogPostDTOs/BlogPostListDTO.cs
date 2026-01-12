namespace PastirmaApi.Application.DTOs.BlogPostDTOs
{
    public class BlogPostListDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Excerpt { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public DateTime? PublishedDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public int ViewCount { get; set; }
        public string ReadTime { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
