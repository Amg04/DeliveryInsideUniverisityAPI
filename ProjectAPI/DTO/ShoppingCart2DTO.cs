using System.ComponentModel.DataAnnotations;

namespace ProjectAPI.DTO
{
    public class ShoppingCart2DTO
    {
        [Required]
        public int ProductId { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Count must be at least 1.")]
        public int Count { get; set; }
    }
}
