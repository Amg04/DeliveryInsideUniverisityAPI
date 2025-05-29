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
        private IOrderItemRepository _orderItemRepository;
        public UnitOfWork(RestaurantAPIContext dbContext)
        {
            this.dbContext = dbContext;
            _repositores = new Hashtable();
        }

        public IGenericRepository<T> Repository<T>() where T : ModelBase
        {
            var key = typeof(T).Name;
            if (!_repositores.ContainsKey(key))
            {
                _repositores.Add(key, new GenericRepository<T>(dbContext));
            }
            return _repositores[key] as IGenericRepository<T>;
        }
        public IOrderRepository OrderRepository
            => _orderRepository ??= new OrderRepository(dbContext);

        public IShoppingCartRepository ShoppingCartRepository
            => _shoppingCartRepository ??= new ShoppingCartRepository(dbContext);

        public IOrderItemRepository OrderItemRepository
           => _orderItemRepository ??= new OrderItemRepository(dbContext);

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
