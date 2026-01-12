using System.ComponentModel.DataAnnotations;

namespace PastirmaApi.Application.DTOs.ProductDTOs
{
    public class UpdateProductsStockRequestDTO
    {
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stok miktarı 0 veya daha büyük olmalıdır")]
        public int Stock { get; set; }
    }
}
