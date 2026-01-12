namespace PastirmaApi.Core.Entities
{
    public class User: BaseEntity
    {
        public string Email { get; set; } = null!;        
        public string? PasswordHash { get; set; }
        public string? Username { get; set; }
        public string? FullName { get; set; }
        public bool IsGuest { get; set; } = true;
        public bool IsVerified { get; set; } = false;
        public DateTime? LastLoginAt { get; set; }
        public UserRole Role { get; set; } = UserRole.Customer;

        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }

        // Account lockout properties (security: prevent brute force attacks)
        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LockoutEnd { get; set; }

        // Navigation properties
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }

    public enum UserRole
    {
        Admin,
        Customer
    }
}

