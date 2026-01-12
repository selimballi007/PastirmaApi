using PastirmaApi.Core.Entities;

namespace PastirmaApi.Application.DTOs.UserDTOs
{
    public class CustomerDTO
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Username { get; set; }
        public string? FullName { get; set; }
        public bool IsVerified { get; set; }
        public bool IsGuest { get; set; }
        public UserRole Role { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalOrders { get; set; }
        public int TotalReviews { get; set; }
    }
}
