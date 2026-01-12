using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PastirmaApi.Core.Entities
{
    public class Notification:BaseEntity
    {
        [Required]
        public int UserId { get; set; } = 0;

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Message { get; set; }

        public NotificationType Type { get; set; } = NotificationType.Info; // info, warning, error, success

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum NotificationType
    {
        Info,
        Warning,
        Error,
        Success
    }
}
