using BLLProject.interfaces;
using DAL.Models;

namespace BAL.interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenaricRepository<T> Repository<T>() where T : class, IModelBase;
        IShoppingCartRepository ShoppingCartRepository { get; }
        IOrderRepository OrderRepository { get; }
        public int Complete();
    }
}
