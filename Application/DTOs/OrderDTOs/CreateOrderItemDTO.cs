using System.ComponentModel.DataAnnotations;

namespace PastirmaApi.Application.DTOs.OrderDTOs
{
    public class CreateOrderItemDTO
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Miktar en az 1 olmalıdır")]
        public int Quantity { get; set; }
    }
}
