using System.ComponentModel.DataAnnotations;

namespace PastirmaApi.Application.DTOs.BlogCategoryDTOs
{
    public class CreateBlogCategoryDTO
    {
        [Required(ErrorMessage = "Kategori adı gereklidir")]
        [StringLength(100, ErrorMessage = "Kategori adı en fazla 100 karakter olabilir")]
        public string Name { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "İkon en fazla 50 karakter olabilir")]
        public string Icon { get; set; } = "📝";

        public int DisplayOrder { get; set; } = 0;
    }
}
