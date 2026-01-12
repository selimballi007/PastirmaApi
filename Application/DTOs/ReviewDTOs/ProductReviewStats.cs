namespace PastirmaApi.Application.DTOs.ReviewDTOs
{
    public class ProductReviewStats
    {
        public int ProductId { get; set; }
        public int TotalReviews { get; set; }
        public double AverageRating { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; } = new();
    }
}
