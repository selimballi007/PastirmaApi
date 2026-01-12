using System.ComponentModel.DataAnnotations;

namespace PastirmaApi.Application.DTOs.CategoryDTOs
{
    public class ReorderCategoryDTO
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int DisplayOrder { get; set; }
    }
}
