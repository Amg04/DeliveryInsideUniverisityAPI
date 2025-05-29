using DAL.Models;
using System.ComponentModel.DataAnnotations;

// use in Add , Edit only
namespace ProjectAPI.DTO.ProductDTOs
{
    public class CreateProductDTO
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string? Description { get; set; }
        [Required]
        public decimal price { get; set; }
        [Required]
        public bool IsAvailable { get; set; }
        [Required]
        public int CategoryId { get; set; }
        [Required]
        public int EstablishmentId { get; set; }
    }

    public static class CreateProductExtensions
    {
        public static CreateProductDTO ToCreateProductDTO(this Product product)
        {
            return new CreateProductDTO
            {
                Name = product.Name,
                CategoryId = product.CategoryId,
                Description = product.Description,
                price = product.price,
                IsAvailable = product.IsAvailable,
                EstablishmentId = product.EstablishmentId,
            };
        }

        public static Product ToCreateProductModel(this CreateProductDTO productDTO, Product product = null)
        {
            if (product == null)
                product = new Product();

            product.Name = productDTO.Name;
            product.CategoryId = productDTO.CategoryId;
            product.Description = productDTO.Description;
            product.price = productDTO.price;
            product.IsAvailable = productDTO.IsAvailable;
            product.EstablishmentId = productDTO.EstablishmentId;

            return product;
        }
    }
}
