using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PastirmaApi.Core.Entities
{
    public class Order : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string OrderNumber { get; set; } = string.Empty;

        // User (nullable for guest checkout)
        public int? UserId { get; set; }
        public User? User { get; set; }

        // Guest customer information (for non-logged-in users)
        [MaxLength(100)]
        public string? GuestName { get; set; }

        [MaxLength(100)]
        public string? GuestEmail { get; set; }

        [MaxLength(20)]
        public string? GuestPhone { get; set; }

        // Addresses
        [Required]
        public int ShippingAddressId { get; set; }
        public Address ShippingAddress { get; set; } = null!;

        public int? BillingAddressId { get; set; }
        public Address? BillingAddress { get; set; }

        // Order amounts
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingCost { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        // Payment
        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        [MaxLength(20)]
        public string? PaymentStatus { get; set; } = "Pending";

        // Order status
        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        // Notes
        [MaxLength(1000)]
        public string? Notes { get; set; }

        // Admin notes (internal)
        [MaxLength(1000)]
        public string? AdminNotes { get; set; }

        // Navigation properties
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

    public enum OrderStatus
    {
        Pending,      // Beklemede
        Confirmed,    // Onaylandı
        Preparing,    // Hazırlanıyor
        Shipped,      // Kargoya Verildi
        Delivered,    // Teslim Edildi
        Returned,     // İade Edildi
        Cancelled     // İptal Edildi
    }

    public enum PaymentMethod
    {
        CreditCard,   // Kredi Kartı
        PayAtDoor     // Kapıda Ödeme
    }
}
