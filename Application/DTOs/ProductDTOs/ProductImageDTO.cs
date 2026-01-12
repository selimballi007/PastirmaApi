using System.ComponentModel.DataAnnotations;

namespace PastirmaApi.Application.DTOs.ProductDTOs
{
    public class ProductImageDTO
    {
        [Required(ErrorMessage = "Görsel URL'i gereklidir")]
        [Url(ErrorMessage = "Geçerli bir URL giriniz")]
        public string ImageUrl { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }

        public bool IsPrimary { get; set; }
    }
}
