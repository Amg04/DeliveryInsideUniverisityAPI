using BLLProject.interfaces;
using DAL;

namespace BAL.interfaces
{
    public interface IOrderRepository : IGenaricRepository<Order>
    {
        void UpdatePaymentStatus(int id, PaymentStatus paymentStatus);
        void UpdateOrderStatus(int id , OrderStatus orderStatus);
    }
}
