namespace PastirmaApi.Application.DTOs.ReviewDTOs
{
    /// <summary>
    /// Global review statistics for dashboard
    /// </summary>
    public class ReviewStats
    {
        public int Pending { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
    }
}
