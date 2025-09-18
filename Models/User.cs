namespace PastirmaApi.Models
{
    public class User: BaseEntity
    {
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public UserRole Role { get; set; } = UserRole.Customer;
    }

    public enum UserRole
    {
        Admin,
        Customer
    }
}

