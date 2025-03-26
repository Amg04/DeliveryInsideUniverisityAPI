using BLLProject.interfaces;
using DAL.Models;

namespace BAL.interfaces
{
    public interface IShoppingCartRepository : IGenaricRepository<ShoppingCart>
    {
        public int DecreaseCount(ShoppingCart ShoppingCart, int count);
        public int IncreaseCount(ShoppingCart ShoppingCart, int count);
        void RemoveRange(IEnumerable<ShoppingCart> entities);
    }
}
