using PastirmaApi.Core.Entities;

namespace PastirmaApi.Application.DTOs.OrderDTOs
{
    public class OrderDTO
    {
        public int? Id { get; set; }
        public string? OrderNumber { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Amount { get; set; }
        public OrderStatus Status { get; set; } 
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
