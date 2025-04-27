using BLLProject.interfaces;
using BLLProject.Specifications;
using DAL;

namespace BAL.interfaces
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        //public void Add(Order entity);
        //public void Update(Order entity); // All
        //public void Delete(Order entity);
        //public Order Get(int Id);
        //public IEnumerable<Order> GetAll();
        //public Order GetEntityWithSpec(ISpecification<Order> spec);
        //public IEnumerable<Order> GetAllWithSpec(ISpecification<Order> spec);
        void UpdatePaymentStatus(int id, PaymentStatus paymentStatus);
        void UpdateOrderStatus(int id , OrderStatus orderStatus);
    }
}
