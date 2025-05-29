using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectAPI.DTO.UserManagementDTOs;
using Utilities;

namespace ProjectAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = SD.AdminRole)]
    public class UserManagementController : ControllerBase
    {
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public UserManagementController(UserManager<User> userManager , RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        [HttpGet("AllUsers")]
        public async Task<IActionResult> Index()
        {
            var users = await userManager.Users.ToListAsync();
            var userDTOs = new List<UserDTO>();

            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);
                userDTOs.Add(new UserDTO
                {
                    Id = user.Id,
                    Name = user.Name,
                    PhoneNumber = user.PhoneNumber ?? "",
                    Email = user.Email,
                    Address = user.Address,
                    Roles = roles
                });
            }

            return Ok(userDTOs);
        }

        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingUser = await userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return BadRequest("A user with this email already exists.");

            var user = new User
            {
                Name = dto.Name,
                UserName = dto.Email.ToUpper(),
                Email = dto.Email,
                Address = dto.Address,
                LockoutEnabled = true
            };

            var result = await userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            foreach (var role in dto.Roles)
            {
                if (!SD.ValidRoles.Contains(role))
                {
                    return BadRequest($"Invalid role: {role}. Valid roles are: {string.Join(", ", SD.ValidRoles)}");
                }

                await userManager.AddToRoleAsync(user, role);
            }

            return Ok(new { message = "User created successfully", userId = user.Id });
        }

        [HttpPost("ToggleLock/{userId}")]
        public async Task<IActionResult> ToggleLock(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            if (user.LockoutEnd != null && user.LockoutEnd > DateTime.Now)
            {
                // Unlock the user
                user.LockoutEnd = null;
                await userManager.UpdateAsync(user);
                return Ok("User unlocked successfully.");
            }
            else
            {
                // Lock the user for 
                user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(50);
                await userManager.UpdateAsync(user);
                return Ok("User locked successfully.");
            }
        }


    }
}
