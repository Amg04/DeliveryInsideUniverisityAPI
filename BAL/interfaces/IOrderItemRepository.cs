using BLLProject.interfaces;
using DAL;
namespace BAL.interfaces
{
    public interface IOrderItemRepository : IGenericRepository<OrderItem>
    {
        void RemoveRange(IEnumerable<OrderItem> entities);
    }
}
