using BLLProject.interfaces;
using BLLProject.Specifications;
using DAL;

namespace BAL.interfaces
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        void UpdatePaymentStatus(int id, PaymentStatus paymentStatus);
        void UpdateOrderStatus(int id , OrderStatus orderStatus);
    }
}
