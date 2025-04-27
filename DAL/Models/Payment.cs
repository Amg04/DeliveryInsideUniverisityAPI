using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Models
{
    public class Payment : ModelBase
    {
        [ForeignKey(nameof(Order))]
        public int OrderId { get; set; }
        public Order Order { get; set; }
        
        public DateTime PaymentTime { get; set; }
        
        // strips Prop
        public string? SessionId { get; set; }
        public string? PaymentIntentId { get; set; }
    }
}
