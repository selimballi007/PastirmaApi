namespace PastirmaApi.Core.Entities
{
    public class User: BaseEntity
    {
        public string Email { get; set; } = null!;

        //Could be null for guest users
        public string? PasswordHash { get; set; }
        public string? Username { get; set; }
        public string? FullName { get; set; }
        public bool IsGuest { get; set; } = true;
        public bool IsVerified { get; set; } = false;
        public DateTime? LastLoginAt { get; set; }
        public bool IsActive { get; set; } = true;
        public UserRole Role { get; set; } = UserRole.Customer;

        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }
    }

    public enum UserRole
    {
        Admin,
        Customer
    }
}

