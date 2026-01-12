using PastirmaApi.Application.DTOs.AddressDTOs;
using PastirmaApi.Core.Entities;

namespace PastirmaApi.Application.DTOs.OrderDTOs
{
    public class OrderDTO
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = null!;

        // Customer info
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public string? GuestName { get; set; }
        public string? GuestEmail { get; set; }
        public string? GuestPhone { get; set; }

        // Addresses
        public AddressDTO? ShippingAddress { get; set; }
        public AddressDTO? BillingAddress { get; set; }

        // Order items
        public List<OrderItemDTO> OrderItems { get; set; } = new();

        // Amounts
        public decimal SubTotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TotalAmount { get; set; }

        // Payment
        public PaymentMethod PaymentMethod { get; set; }
        public string? PaymentStatus { get; set; }

        // Status
        public OrderStatus OrderStatus { get; set; }

        // Notes
        public string? Notes { get; set; }
        public string? AdminNotes { get; set; }

        // Dates
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
