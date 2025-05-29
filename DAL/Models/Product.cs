using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DAL.Models
{
    public class Product : ModelBase
    {
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string? Description { get; set; }
        public decimal price { get; set; }
        public bool IsAvailable { get; set; }
       
        [ForeignKey(nameof(Category))]
        public int  CategoryId { get; set; }
        [JsonIgnore]
        public Category Category { get; set; }
        
        [ForeignKey(nameof(Establishment))]
        public int EstablishmentId { get; set; }
        [JsonIgnore]
        public Establishment Establishment { get; set; }
        //[JsonIgnore]
        //public ICollection<OrderItem> Orders { get; set; } = new HashSet<OrderItem>();
    }
}
