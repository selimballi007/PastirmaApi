using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PastirmaApi.Core.Entities
{
    public class Order:BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string OrderNumber { get; set; } = string.Empty;

        [Required]
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(20)]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        // Navigation properties
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

    public enum OrderStatus
    {
        Pending,
        Processing,
        Completed,
        Cancelled,
        Refunded
    }
}
