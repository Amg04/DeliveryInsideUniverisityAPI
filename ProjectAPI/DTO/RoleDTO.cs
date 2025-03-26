using System.ComponentModel.DataAnnotations;

namespace ProjectAPI.DTO
{
    public class RoleDTO
    {

        [Display(Name = "Role Name")]
        public string RoleName { get; set; }
    }
}
