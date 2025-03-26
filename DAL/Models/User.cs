using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DAL.Models
{
    public class User : IdentityUser, IModelBase
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public ICollection<Order> Orders { get; set; } = new HashSet<Order>();
    }
}
