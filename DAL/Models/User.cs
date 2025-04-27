using Microsoft.AspNetCore.Identity;
namespace DAL.Models
{
    public class User : IdentityUser
    {
        public string Name { get; set; }
        public string Address { get; set; }
        //public ICollection<Order> Orders { get; set; } = new HashSet<Order>();
    }
}
