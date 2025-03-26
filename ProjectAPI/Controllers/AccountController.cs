using DAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProjectAPI.DTO;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
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
                UserName = UserFromRequest.Email
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
                new Claim(ClaimTypes.Name, UserFromDB.UserName)
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
    }
}