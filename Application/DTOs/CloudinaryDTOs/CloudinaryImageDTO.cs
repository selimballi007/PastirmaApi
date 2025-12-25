namespace PastirmaApi.Application.DTOs.CloudinaryDTOs
{
    public class CloudinaryImageDTO
    {
        public string PublicId { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string SecureUrl { get; set; } = string.Empty;
        public string Format { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
        public long Bytes { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ResourceType { get; set; } = string.Empty;

        // Database usage tracking
        public List<ProductUsageDTO> UsedInProducts { get; set; } = new List<ProductUsageDTO>();
        public bool IsUsedInDatabase { get; set; }
    }

    public class ProductUsageDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string UsageType { get; set; } = string.Empty; // "MainImage" or "GalleryImage"
    }
}
