using System.ComponentModel.DataAnnotations;

namespace ProjectAPI.DTO
{
    public class ShoppingCartDTO
    {
        [Required]
        public int ProductId { get; set; }
        [Required]
        [Range(1, 100, ErrorMessage = "Count must be between 1 and 100.")]
        public int Count { get; set; }
    }
}
