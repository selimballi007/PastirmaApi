using System.ComponentModel.DataAnnotations;
using PastirmaApi.Application.DTOs.AddressDTOs;
using PastirmaApi.Core.Entities;

namespace PastirmaApi.Application.DTOs.OrderDTOs
{
    public class CreateOrderDTO
    {
        // Guest information (required if not logged in)
        [MaxLength(100)]
        public string? GuestName { get; set; }

        [MaxLength(100)]
        [EmailAddress]
        public string? GuestEmail { get; set; }

        [MaxLength(20)]
        public string? GuestPhone { get; set; }

        // Addresses
        [Required(ErrorMessage = "Teslimat adresi zorunludur")]
        public CreateAddressDTO ShippingAddress { get; set; } = null!;

        public CreateAddressDTO? BillingAddress { get; set; }

        // Order items
        [Required(ErrorMessage = "Sipariş öğeleri zorunludur")]
        [MinLength(1, ErrorMessage = "En az bir ürün eklemelisiniz")]
        public List<CreateOrderItemDTO> OrderItems { get; set; } = new();

        // Payment
        [Required(ErrorMessage = "Ödeme yöntemi zorunludur")]
        public PaymentMethod PaymentMethod { get; set; }

        // Notes
        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}
