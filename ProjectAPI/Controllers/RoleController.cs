using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjectAPI.DTO.RoleDTOs;
using Utilities;

namespace ProjectAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = SD.AdminRole)]
    public class RoleController : ControllerBase
    {
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signIn;
        private readonly RoleManager<IdentityRole> roleManager;

        public RoleController(UserManager<User> userManager, SignInManager<User> SignIn,
            RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            signIn = SignIn;
            this.roleManager = roleManager;
        }

        [HttpGet("AllRoles")]
        public  IActionResult GetAllRoles()
        {
            var roles = roleManager.Roles.Select(r => new
            {
                r.Id,
                r.Name
            }).ToList();

            return Ok(roles);
        }

        [HttpGet("GetUserRoles")]
        public async Task<IActionResult> GetUserRoles([FromQuery] string UserEmail)
        {
            var user = await userManager.FindByEmailAsync(UserEmail);
            if (user == null)
                return NotFound(new { Message = "User not found" });

            var roles = await userManager.GetRolesAsync(user);

            return Ok(roles);
        }


        [HttpPost("AddRole")]
        public async Task<IActionResult> AddRole([FromBody] RoleDTO role)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // check if dto is exist 
            bool roleExists = await roleManager.RoleExistsAsync(role.RoleName);
            if (roleExists)
                return BadRequest(new { Message = "Role already exists" });

            // save DB
            IdentityRole roleModel = new IdentityRole()
            {
                Name = role.RoleName,
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };
            IdentityResult result = await roleManager.CreateAsync(roleModel);
            if (result.Succeeded == true)
                return Ok(new { Message = "Role created successfully", RoleName = role.RoleName });

            return BadRequest(result.Errors.Select(e => e.Description));
        }

      
        [HttpDelete("RemoveRole")]
        public async Task<IActionResult> RemoveRole([FromQuery] RoleDTO dto)
        {
            var role = await roleManager.FindByNameAsync(dto.RoleName);
            if (role == null)
                return NotFound(new { Message = "Role not found" });

            var result = await roleManager.DeleteAsync(role);
            if (result.Succeeded)
                return Ok(new { Message = $"Role '{dto.RoleName}' deleted successfully." });

            return BadRequest(result.Errors.Select(e => e.Description));
        }



        [HttpPost("AssignRole")]
        public async Task<IActionResult> AssignRole([FromBody] UserRoleDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var roleExists = await roleManager.RoleExistsAsync(dto.RoleName);
            if (!roleExists)
            {
                return BadRequest(new { Message = "Role does not exist" });
            }

            var user = await userManager.FindByEmailAsync(dto.UserEmail);
            if (user == null)
                return NotFound(new { Message = "User not found" });

            if (await userManager.IsInRoleAsync(user, dto.RoleName))
                return BadRequest(new { Message = "User already assigned to this dto" });

            var result = await userManager.AddToRoleAsync(user, dto.RoleName);
            if (result.Succeeded)
                return Ok(new { Message = $"Role '{dto.RoleName}' assigned to user '{dto.UserEmail}'" });

            return BadRequest(result.Errors.Select(e => e.Description));
        }

        [HttpPost("RemoveRoleFromUser")]
        public async Task<IActionResult> RemoveRoleFromUser([FromBody] UserRoleDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await userManager.FindByEmailAsync(dto.UserEmail);
            if (user == null)
                return NotFound(new { Message = "User not found" });

            var roleExists = await roleManager.RoleExistsAsync(dto.RoleName);
            if (!roleExists)
                return NotFound(new { Message = "Role not found" });

            var isInRole = await userManager.IsInRoleAsync(user, dto.RoleName);
            if (!isInRole)
                return BadRequest(new { Message = "User is not assigned to this dto" });

            var result = await userManager.RemoveFromRoleAsync(user, dto.RoleName);
            if (result.Succeeded)
                return Ok(new { Message = $"Role '{dto.RoleName}' removed from user '{dto.UserEmail}' successfully." });

            return BadRequest(result.Errors.Select(e => e.Description));
        }

        [HttpPost("UpdateUserRoles")]
        public async Task<IActionResult> UpdateUserRoles([FromBody] UpdateUserRolesDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await userManager.FindByEmailAsync(dto.UserEmail);
            if (user == null)
                return NotFound(new { Message = "User not found" });

            var currentRoles = await userManager.GetRolesAsync(user);

            // Remove all current roles
            var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
                return BadRequest(removeResult.Errors.Select(e => e.Description));

            // Filter roles to only those that exis
            var validRoles = new List<string>();
            var invalidRoles = new List<string>();

            foreach (var role in dto.Roles)
            {
                if (await roleManager.RoleExistsAsync(role))
                    validRoles.Add(role);
                else
                    invalidRoles.Add(role);
            }

            if (invalidRoles.Any())
                return BadRequest(new { Message = "Some roles are invalid", InvalidRoles = invalidRoles });

            // Assign new roles
            var addResult = await userManager.AddToRolesAsync(user, validRoles);
            if (!addResult.Succeeded)
                return BadRequest(addResult.Errors.Select(e => e.Description));

            return Ok(new
            {
                Message = $"User roles updated successfully for {dto.UserEmail}.",
                NewRoles = validRoles
            });
        }


    }
}
