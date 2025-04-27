using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DAL.Models
{
    public class Establishment : ModelBase
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public string OpeningHours { get; set; }
        public string ContactNumber { get; set; }
        public string ImageUrl { get; set; }
        public EsbCategoryType EsbCategory { get; set; }
       
        [ForeignKey(nameof(Category))]
        public int CategoryId { get; set; }
        [JsonIgnore]
        public Category Category { get; set; }

        //[JsonIgnore]
        //public ICollection<Order> Orders { get; set; } = new HashSet<Order>();
        [JsonIgnore]
        public ICollection<Product> Products { get; set; } = new HashSet<Product>();
    }

    public enum EsbCategoryType
    {
        restaurant=0,
        cafe=1,
        grocerystore=2,
        bookstore=3
    }

}
