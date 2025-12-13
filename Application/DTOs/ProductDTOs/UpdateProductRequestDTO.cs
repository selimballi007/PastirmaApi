using System.ComponentModel.DataAnnotations;

namespace PastirmaApi.Application.DTOs.ProductDTOs
{
    public class UpdateProductRequestDTO
    {
        [Required(ErrorMessage = "Ürün adı gereklidir")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Ürün adı 3-200 karakter arasında olmalıdır")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Fiyat gereklidir")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalıdır")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stok miktarı gereklidir")]
        [Range(0, int.MaxValue, ErrorMessage = "Stok miktarı 0 veya daha büyük olmalıdır")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "Kategori gereklidir")]
        public int CategoryId { get; set; }

        [Url(ErrorMessage = "Geçerli bir URL giriniz")]
        [StringLength(500, ErrorMessage = "Görsel URL'i en fazla 500 karakter olabilir")]
        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; }
    }
}
