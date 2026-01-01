namespace PastirmaApi.Application.DTOs.AddressDTOs
{
    public class AddressDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string AddressLine1 { get; set; } = null!;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = null!;
        public string District { get; set; } = null!;
        public string? PostalCode { get; set; }
        public string? Notes { get; set; }
        public int? UserId { get; set; }
    }
}
