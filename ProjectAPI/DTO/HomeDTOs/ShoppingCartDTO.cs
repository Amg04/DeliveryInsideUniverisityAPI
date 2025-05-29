using DAL.Models;
using ProjectAPI.DTO.ProductDTOs;
using System.ComponentModel.DataAnnotations;

namespace ProjectAPI.DTO.HomeDTOs
{
    public class ShoppingCartDTO
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string CategoryName { get; set; }
        [Range(1, 100, ErrorMessage = "Count must be between 1 and 100.")]
        public int Count { get; set; } = 1;
    }

    public static class UserExtensions
    {
        public static ShoppingCartDTO ToShoppingCartDTO(this ShoppingCart shoppingCart )
        {
            return new ShoppingCartDTO
            {
                ProductId = shoppingCart.Product.id,
                Name = shoppingCart.Product.Name,
                Price = shoppingCart.Product.price,
                CategoryName = shoppingCart.Product.Category.Name,
            };
        }
    }
}
