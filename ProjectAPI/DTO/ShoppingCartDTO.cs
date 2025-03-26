using DAL.Models;

namespace ProjectAPI.DTO
{
    public class ShoppingCartDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string UserId { get; set; }
        public int Count { get; set; }
        public ProductDTO Product { get; set; }
    }

    public static class ShoppingCartExtensions
    {
        public static ShoppingCartDTO ToShoppingCartDTO(this ShoppingCart cart)
        {
            return new ShoppingCartDTO
            {
                Id = cart.id,
                ProductId = cart.ProductId,
                UserId = cart.UserId,
                Count = cart.Count,
                Product = cart.Product?.ToProductDTO() 
            };
        }

        public static ShoppingCart ToShoppingCartModel(this ShoppingCartDTO cartDTO, ShoppingCart cart = null)
        {
            if (cart == null)
                cart = new ShoppingCart();

            cart.id = cartDTO.Id;
            cart.ProductId = cartDTO.ProductId;
            cart.UserId = cartDTO.UserId;
            cart.Count = cartDTO.Count;

            return cart;
        }
    }
}
