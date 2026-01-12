using System.ComponentModel.DataAnnotations;

namespace PastirmaApi.Core.Entities
{
    public class Address : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        public string Phone { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string AddressLine1 { get; set; } = null!;

        [MaxLength(200)]
        public string? AddressLine2 { get; set; }

        [Required]
        [MaxLength(100)]
        public string City { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string District { get; set; } = null!;

        [MaxLength(10)]
        public string? PostalCode { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        // Optional: Link to user if they're logged in
        public int? UserId { get; set; }
        public User? User { get; set; }

        // Is this address the default address for the user
        public bool IsDefault { get; set; } = false;
    }
}
