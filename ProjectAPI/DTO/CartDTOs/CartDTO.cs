using BAL.interfaces;
using DAL.Models;
using DAL;

namespace ProjectAPI.DTO.CartDTOs
{
    public class CartDTO
    {
        public IEnumerable<ShoppingCart> CartsList { get; set; }
        public decimal TotalCarts { get; set; }
        public OrderDTO order { get; set; }
    }
}
