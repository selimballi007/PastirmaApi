using System.ComponentModel.DataAnnotations;

namespace PastirmaApi.Application.DTOs.BlogPostDTOs
{
    public class UpdateBlogPostDTO
    {
        [Required(ErrorMessage = "Başlık gereklidir")]
        [StringLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "İçerik gereklidir")]
        [StringLength(5000, ErrorMessage = "İçerik en fazla 5000 karakter olabilir")]
        public string Content { get; set; } = string.Empty;

        [Required(ErrorMessage = "Özet gereklidir")]
        [StringLength(500, ErrorMessage = "Özet en fazla 500 karakter olabilir")]
        public string Excerpt { get; set; } = string.Empty;

        [Required(ErrorMessage = "Görsel URL gereklidir")]
        public string ImageUrl { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kategori seçiniz")]
        public int CategoryId { get; set; }

        public DateTime? PublishedDate { get; set; }

        public bool IsFeatured { get; set; } = false;
    }
}
