using BAL.interfaces;
using BLLProject.Repositories;
using BLLProject.Specifications;
using DAL;
using DAL.Data;
using DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
namespace BAL.Repositories
{
    class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        private readonly RestaurantAPIContext context;

        public OrderRepository(RestaurantAPIContext context) : base(context)
        {
            this.context = context;
        }

        //public void Add(Order entity)
        //{
        //    dbContext.Set<Order>().Add(entity);
        //}

        //public void Update(Order entity)
        //{
        //    dbContext.Set<Order>().Update(entity);
        //}

        //public void Delete(Order entity)
        //{
        //    dbContext.Set<Order>().Remove(entity);
        //}

        //public Order Get(int Id) => dbContext.Set<Order>().Find(Id);
        //public IEnumerable<Order> GetAll() => dbContext.Set<Order>().AsNoTracking().ToList();

        //public Order GetEntityWithSpec(ISpecification<Order> spec) =>
        //   ApplySpecification(spec).FirstOrDefault();
        //public IEnumerable<Order> GetAllWithSpec(ISpecification<Order> spec) =>
        //    ApplySpecification(spec).AsNoTracking().ToList();

        //private IQueryable<Order> ApplySpecification(ISpecification<Order> spec) =>
        //    SpecificationEvalutor<Order>.GetQuery(dbContext.Set<Order>(), spec);

        public void UpdatePaymentStatus(int id, PaymentStatus paymentStatus)
        {
            var OrderFromDb = context.Order.FirstOrDefault(x => x.id == id);
            var paymentMethod = context.Payment.FirstOrDefault(x => x.OrderId == id);
            if (OrderFromDb != null)
            {
                if (paymentMethod != null)
                {
                    paymentMethod.PaymentTime = DateTime.Now;
                }
                OrderFromDb.PaymentStatus = paymentStatus;
            }
        }

        public void UpdateOrderStatus(int id, OrderStatus orderStatus)
        {
            var OrderFromDb = context.Order.FirstOrDefault(x => x.id == id);
            if (OrderFromDb != null)
                OrderFromDb.OrderStatus = orderStatus;
        }
    }
}
