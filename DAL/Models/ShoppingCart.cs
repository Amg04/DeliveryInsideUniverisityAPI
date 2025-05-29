using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DAL.Models
{
    public class ShoppingCart : ModelBase
    {
        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }
        public Product Product { get; set; }
       
        [ForeignKey(nameof(User))]
        public string UserId { get; set; }
        [JsonIgnore]
        public User User { get; set; }

        public int Count { get; set; }
    }
}
