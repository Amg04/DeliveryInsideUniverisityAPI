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


        //public void Add(ShoppingCart entity)
        //{
        //    dbContext.Set<ShoppingCart>().Add(entity);
        //}

        //public void Update(ShoppingCart entity)
        //{
        //    dbContext.Set<ShoppingCart>().Update(entity);
        //}

        //public void Delete(ShoppingCart entity)
        //{
        //    dbContext.Set<ShoppingCart>().Remove(entity);
        //}

        //public ShoppingCart Get(int Id) => dbContext.Set<ShoppingCart>().Find(Id);
        //public IEnumerable<ShoppingCart> GetAll() => dbContext.Set<ShoppingCart>().AsNoTracking().ToList();

        //public ShoppingCart GetEntityWithSpec(ISpecification<ShoppingCart> spec) =>
        //   ApplySpecification(spec).FirstOrDefault();
        //public IEnumerable<ShoppingCart> GetAllWithSpec(ISpecification<ShoppingCart> spec) =>
        //    ApplySpecification(spec).AsNoTracking().ToList();

        //private IQueryable<ShoppingCart> ApplySpecification(ISpecification<ShoppingCart> spec) =>
        //    SpecificationEvalutor<ShoppingCart>.GetQuery(dbContext.Set<ShoppingCart>(), spec);

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
