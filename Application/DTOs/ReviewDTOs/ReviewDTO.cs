namespace PastirmaApi.Application.DTOs.ReviewDTO
{
    public class ReviewDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }
}
