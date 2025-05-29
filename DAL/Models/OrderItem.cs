using DAL.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL
{
    public class OrderItem : ModelBase
    {
        [ForeignKey(nameof(Order))]
        public int OrderId { get; set; }
        //public Order Order { get; set; }
        
        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }
        public Product Product { get; set; }
        
        public int Quantity { get; set; }
        public decimal SubTotal { get; set; }
    }
}
