using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PastirmaApi.Core.Entities
{
    public class Review:BaseEntity
    {
        [Required]
        public int ProductId { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }
        public string? Comment { get; set; }

        public Product Product { get; set; } = null!;
        public User User { get; set; } = null!;
        public DateTime ApprovedDate { get; set; }
        public ReviewStatus Status { get; set; }
    }

    public enum ReviewStatus
    {
        Pending,
        Approved,
        Rejected
    }
}
