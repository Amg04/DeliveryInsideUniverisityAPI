using System.ComponentModel.DataAnnotations;

namespace ProjectAPI.DTO
{
    public class CategoryDTO
    {
        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}
