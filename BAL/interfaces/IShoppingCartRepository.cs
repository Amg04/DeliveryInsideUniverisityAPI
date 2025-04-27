using BLLProject.interfaces;
using DAL.Models;

namespace BAL.interfaces
{
    public interface IShoppingCartRepository : IGenericRepository<ShoppingCart>
    {
        //public void Add(ShoppingCart entity);
        //public void Update(ShoppingCart entity); // All
        //public void Delete(ShoppingCart entity);
        //public ShoppingCart Get(int Id);
        //public IEnumerable<ShoppingCart> GetAll();
        //public ShoppingCart GetEntityWithSpec(ISpecification<ShoppingCart> spec);
        //public IEnumerable<ShoppingCart> GetAllWithSpec(ISpecification<ShoppingCart> spec);
        public int DecreaseCount(ShoppingCart ShoppingCart, int count);
        public int IncreaseCount(ShoppingCart ShoppingCart, int count);
        void RemoveRange(IEnumerable<ShoppingCart> entities);
    }
}
