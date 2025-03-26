using BAL.interfaces;
using BAL.Repositories;
using BLLProject.interfaces;
using DAL.Data;
using DAL.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections;


namespace BLLProject.Repositories
{
     
    public class UnitOfWork : IUnitOfWork
    {
        private readonly RestaurantAPIContext dbContext;
        private Hashtable _repositores;
        private IShoppingCartRepository _shoppingCartRepository;
        private IOrderRepository _orderRepository;
        public UnitOfWork(RestaurantAPIContext dbContext)
        {
            this.dbContext = dbContext;
            _repositores = new Hashtable();
        }

        public IGenaricRepository<T> Repository<T>() where T : class, IModelBase
        {
            var key = typeof(T).Name;
            if (!_repositores.ContainsKey(key))
            {
                _repositores.Add(key, new GenericRepository<T>(dbContext));
            }
            return _repositores[key] as IGenaricRepository<T>;
        }
        public IOrderRepository OrderRepository
            => _orderRepository ??= new OrderRepository(dbContext);

        public IShoppingCartRepository ShoppingCartRepository
            => _shoppingCartRepository ??= new ShoppingCartRepository(dbContext);

        public int Complete()
        {
            return dbContext.SaveChanges();
        }

        public void Dispose()
        {
            dbContext.Dispose();
        }
    }
}
