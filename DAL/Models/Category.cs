using System.Text.Json.Serialization;

namespace DAL.Models
{
    public class Category : ModelBase
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        //public ICollection<Product> Products { get; set; } = new HashSet<Product>();
        //public ICollection<Establishment> Establishments { get; set; } = new HashSet<Establishment>();

    }
}
