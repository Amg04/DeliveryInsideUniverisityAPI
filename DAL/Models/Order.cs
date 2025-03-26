using DAL.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL
{
    public class Order : IModelBase
    {
        public int id { get; set; }

        [ForeignKey(nameof(User))]
        public string UserId { get; set; }
        public User User { get; set; }
       
        public decimal TotalPrice { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ShippingDate { get; set; } 
        
        public OrderStatus OrderStatus { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        [ForeignKey(nameof(Establishment))]
        public int EstablishmentId { get; set; }
        public Establishment Establishment { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new HashSet<OrderItem>();
        public Payment Payment { get; set; }

        //Data of User
        public string Name { get; set; }
        public string Address { get; set; }
        public string? PhoneNumber { get; set; }

    }

    public enum OrderStatus
    {
        Pending = 0,
        Preparing = 1,
        Prepared = 2,
        OnTheWay = 3,
        Delivered = 4,
        Canceled = 5
    }

    public enum PaymentStatus
    {
        Pending = 0,
        Approved = 1,
        CashOnDelivery = 2,
        Completed = 3,
        Failed = 4,
        Refund = 5
    }




}
