using System.ComponentModel.DataAnnotations;

namespace ProjectAPI.DTO.CategoryDTOs
{
    public class CreateCategoryDTO
    {
        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}
