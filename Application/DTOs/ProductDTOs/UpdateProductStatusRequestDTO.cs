using System.ComponentModel.DataAnnotations;

namespace PastirmaApi.Application.DTOs.ProductDTOs
{
    public class UpdateProductStatusRequestDTO
    {
        [Required]
        public bool IsActive { get; set; }
    }
}
