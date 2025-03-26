using BAL.interfaces;
using BLLProject.Repositories;
using DAL.Data;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BAL.Repositories
{
    public class ShoppingCartRepository :GenericRepository<ShoppingCart>, IShoppingCartRepository
    {
        private readonly RestaurantAPIContext context;

        public ShoppingCartRepository(RestaurantAPIContext context) : base(context)
        {
            this.context = context;
        }
        public int DecreaseCount(ShoppingCart ShoppingCart, int count)
        {
            ShoppingCart.Count -= count;
            return ShoppingCart.Count;
        }

        public int IncreaseCount(ShoppingCart ShoppingCart, int count)
        {
            ShoppingCart.Count += count;
            return ShoppingCart.Count;
        }

        public void RemoveRange(IEnumerable<ShoppingCart> entities)
        {
            context.RemoveRange(entities);
        }

    }
}
