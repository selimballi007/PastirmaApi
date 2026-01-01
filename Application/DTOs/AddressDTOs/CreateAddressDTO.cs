using System.ComponentModel.DataAnnotations;

namespace PastirmaApi.Application.DTOs.AddressDTOs
{
    public class CreateAddressDTO
    {
        [Required(ErrorMessage = "Ad Soyad zorunludur")]
        [MaxLength(100)]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Telefon zorunludur")]
        [MaxLength(20)]
        public string Phone { get; set; } = null!;

        [Required(ErrorMessage = "Adres zorunludur")]
        [MaxLength(200)]
        public string AddressLine1 { get; set; } = null!;

        [MaxLength(200)]
        public string? AddressLine2 { get; set; }

        [Required(ErrorMessage = "İl zorunludur")]
        [MaxLength(100)]
        public string City { get; set; } = null!;

        [Required(ErrorMessage = "İlçe zorunludur")]
        [MaxLength(100)]
        public string District { get; set; } = null!;

        [MaxLength(10)]
        public string? PostalCode { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
