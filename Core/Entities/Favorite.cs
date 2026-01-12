using System.ComponentModel.DataAnnotations;

namespace PastirmaApi.Core.Entities
{
    public class Favorite : BaseEntity
    {
        [Required]
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        [Required]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;
    }
}
