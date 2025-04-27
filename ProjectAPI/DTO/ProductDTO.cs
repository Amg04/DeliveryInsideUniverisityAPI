using DAL.Models;
using System.ComponentModel.DataAnnotations;

namespace ProjectAPI.DTO
{
    public class ProductDTO 
    {
        public int id { get; set; }
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
        public string ImageUrl { get; set; }
    }

    public static class ProductExtensions
    {
        public static ProductDTO ToProductDTO(this Product product)
        {
            return new ProductDTO
            {
                id = product.id,
                Name = product.Name,
                CategoryId = product.CategoryId,
                Description = product.Description,
                price = product.price,
                IsAvailable = product.IsAvailable,
                EstablishmentId = product.EstablishmentId,
                ImageUrl = product.ImageUrl,
            };
        }

        public static Product ToProductModel(this ProductDTO productDTO, Product product = null)
        {
            if (product == null)
                product = new Product();

            product.Name = productDTO.Name;
            product.CategoryId = productDTO.CategoryId;
            product.Description = productDTO.Description;
            product.price = productDTO.price;
            product.IsAvailable = productDTO.IsAvailable;
            product.EstablishmentId = productDTO.EstablishmentId;
            product.ImageUrl = productDTO.ImageUrl;
            return product;
        }
    }


}
