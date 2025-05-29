using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ProjectAPI.DTO.AccountDTOs;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using Utilities;

namespace ProjectAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signIn;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration configuration; // AppSettings دي اللي بتقرا من ال

        public AccountController(UserManager<User> userManager, SignInManager<User> SignIn,
            RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            this.userManager = userManager;
            signIn = SignIn;
            this.roleManager = roleManager;
            this.configuration = configuration;
        }

        [HttpPost("Register")]  //post /api/Account
        public async Task<IActionResult> Register(RegisterDTO UserFromRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            User AppUser = new User()
            {
                Name = UserFromRequest.Name,
                Email = UserFromRequest.Email,
                Address = UserFromRequest.Address,
                UserName = new MailAddress(UserFromRequest.Email).User
            };

            IdentityResult result = await userManager.CreateAsync(AppUser, UserFromRequest.Password);
            if (result.Succeeded)
            {
                // assign to role

                // Assign all available roles
                //var allRoles = await roleManager.Roles.Select(r => r.Name).ToListAsync(); // All Role 
                //await userManager.AddToRolesAsync(AppUser, allRoles); // give it All Role 
                //await signIn.SignInAsync(AppUser, isPersistent: false);

                //Assign one role
               await userManager.AddToRoleAsync(AppUser, SD.CustomerRole);
                await signIn.SignInAsync(AppUser, isPersistent: false);

                return Ok(new { message = "User Created Successfully", User = AppUser.Name });
            }
            // if not Succeeded
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return BadRequest(ModelState);
        }


        [HttpPost("Login")] //post /api/Account
        public async Task<IActionResult> Login(LoginDTO UserFromRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check هتاكد انة موجودد
            User UserFromDB = await userManager.FindByEmailAsync(UserFromRequest.Email); // Email
            if (UserFromDB == null || !await userManager.CheckPasswordAsync(UserFromDB, UserFromRequest.Password))
            {
                return Unauthorized(new { message = "Invalid Email or Password" });
            }


            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, UserFromDB.Id),
                new Claim(ClaimTypes.Name, UserFromDB.UserName),
            };
            var UserRoles = await userManager.GetRolesAsync(UserFromDB);
            userClaims.AddRange(UserRoles.Select(role => new Claim(ClaimTypes.Role, role)));

            // login دة اللي انا عاوزة يتغير مع كل 
            // Token Generated id Change (JWT predefined Claims)
            userClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())); // Token id 

            // key 
            var SignInKey = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(configuration["JWT:SecretKey"]));
            SigningCredentials signingCred = new SigningCredentials(SignInKey, SecurityAlgorithms.HmacSha256);


            // design token 
            JwtSecurityToken myToken = new JwtSecurityToken(
                issuer: configuration["JWT:IssuerIP"],
                audience: configuration["JWT:AudienceIP"],
                expires: DateTime.UtcNow.AddMonths(6),
                claims: userClaims,
                signingCredentials: signingCred
                );

            // generate Token response
            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(myToken),
                expiration = myToken.ValidTo
            });
        }

        [Authorize]
        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // id
            if (userId == null)
                return Unauthorized("User not found in token");

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("User not found");

            var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
                return Ok(new { message = "Password changed successfully" });

            return BadRequest(result.Errors.Select(e => e.Description));
        }


        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized("User ID not found in token");
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("User not found");
            var userDTO = user.ToUserDTO();
            return Ok(userDTO);
        }
        
        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> EditProfile(profileDTO model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized("User ID not found in token");
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("User not found");

            user.Name = model.Name;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;
            await userManager.UpdateAsync(user);
            return Ok("Profile was updated successfully");
        }

    }
}