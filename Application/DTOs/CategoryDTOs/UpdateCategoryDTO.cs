using System.ComponentModel.DataAnnotations;

namespace PastirmaApi.Application.DTOs.CategoryDTOs
{
    public class UpdateCategoryDTO
    {
        [Required(ErrorMessage = "Kategori adı zorunludur.")]
        [MaxLength(100, ErrorMessage = "Kategori adı en fazla 100 karakter olabilir.")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(10, ErrorMessage = "Icon en fazla 10 karakter olabilir.")]
        public string? Icon { get; set; }
    }
}
