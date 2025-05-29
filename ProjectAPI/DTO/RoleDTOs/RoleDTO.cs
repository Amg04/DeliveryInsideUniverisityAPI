using System.ComponentModel.DataAnnotations;

namespace ProjectAPI.DTO.RoleDTOs
{
    public class RoleDTO
    {

        [Display(Name = "Role Name")]
        public string RoleName { get; set; }
    }
}
