using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjectAPI.DTO;
using Utilities;

namespace ProjectAPI.Controllers
{
    [Authorize(Roles = SD.AdminRole)]
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> roleManager;
        public RolesController(RoleManager<IdentityRole> roleManager)
        {
            this.roleManager = roleManager;
        }

        [HttpPost]
        public async Task<IActionResult> AddRole(RoleDTO role)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // save DB
            IdentityRole roleModel = new IdentityRole();
            roleModel.Name = role.RoleName;
            IdentityResult result = await roleManager.CreateAsync(roleModel);
            if (result.Succeeded == true)
            {
                return Ok(new { Message = "Role created successfully", Role = roleModel });
            }

            return BadRequest(result.Errors.Select(e => e.Description));
        }
    }
}


