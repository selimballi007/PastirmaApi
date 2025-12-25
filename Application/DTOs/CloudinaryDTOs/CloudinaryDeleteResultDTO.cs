namespace PastirmaApi.Application.DTOs.CloudinaryDTOs
{
    public class CloudinaryDeleteResultDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<ProductUpdateDTO> UpdatedProducts { get; set; } = new List<ProductUpdateDTO>();
    }

    public class ProductUpdateDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string UpdateType { get; set; } = string.Empty; // "MainImageRemoved" or "GalleryImageRemoved"
    }
}
