using DAL.Models;
using System.ComponentModel.DataAnnotations;

namespace ProjectAPI.DTO
{
    public class CategoryDTO
    {
        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }
    }

    public static class CategoryExtensions
    {
        // تحويل من Category إلى CategoryDTO
        public static CategoryDTO ToCategoryDTO(this Category category)
        {
            return new CategoryDTO
            {
                Name = category.Name,
                Description = category.Description
            };
        }

        // تحويل من CategoryDTO إلى Category
        public static Category ToCategoryModel(this CategoryDTO categoryDTO, Category category = null)
        {
            if (category == null)
                category = new Category(); 

            category.Name = categoryDTO.Name;
            category.Description = categoryDTO.Description;

            return category;
        }
    }



}
