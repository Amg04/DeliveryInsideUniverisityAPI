using BAL.interfaces;
using BLLProject.Repositories;
using DAL;
using DAL.Data;
namespace BAL.Repositories
{
    public class OrderItemRepository : GenericRepository<OrderItem>, IOrderItemRepository
    {
        private readonly RestaurantAPIContext context;

        public OrderItemRepository(RestaurantAPIContext context) : base(context)
        {
            this.context = context;
        }
        public void RemoveRange(IEnumerable<OrderItem> entities)
        {
            context.RemoveRange(entities);
        }

       
    }
}
