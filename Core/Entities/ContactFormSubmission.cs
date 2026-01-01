using System.ComponentModel.DataAnnotations;

namespace PastirmaApi.Core.Entities
{
    public class ContactFormSubmission : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = null!;

        [MaxLength(20)]
        public string? Phone { get; set; }

        [Required]
        [MaxLength(100)]
        public string Subject { get; set; } = null!;

        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = null!;

        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }
        public bool IsReplied { get; set; } = false;
        public DateTime? RepliedAt { get; set; }
        public string? Notes { get; set; }
    }
}
