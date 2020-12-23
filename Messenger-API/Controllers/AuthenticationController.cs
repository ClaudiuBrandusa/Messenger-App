using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Messenger_API.Authentication;
using Messenger_API.Data;
using Messenger_API.Filters;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Messenger_API.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]

    public class AuthenticationController : Controller 
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration _configuration;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly MessageContext messageContext;

        public AuthenticationController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration,  
                                        SignInManager<ApplicationUser> signInManager, MessageContext messageContext)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.messageContext = messageContext;
            _configuration = configuration;
            _signInManager = signInManager;
        }

        [HttpPost("Register")]
        [APIKeyAuth]
        public async Task<IActionResult> Register([FromForm] Register model)
        {
            var userExist = await userManager.FindByNameAsync(model.UserName);

            if (userExist != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User could not be created" });

            ApplicationUser user = new ApplicationUser()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.UserName
            };

            var result = await userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User could not be created" });
            }

            messageContext.SmallUsers.Add(new Models.SmallUser { UserId = user.SecurityStamp, UserName = user.UserName });
            await messageContext.SaveChangesAsync();

            return Ok(new Response { Status = "Success", Message = "User created successfully" });
        }

        [HttpPost("Login")]
        [APIKeyAuth]
        public async Task<IActionResult> Login([FromForm] Login model)
        {
            var user = await userManager.FindByNameAsync(model.Username);
            if(user != null && await userManager.CheckPasswordAsync(user,model.Password))
            {
                var userRoles = await userManager.GetRolesAsync(user);
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name,user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };
                
                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var authSigninKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:ApiKey"]));
                var token = new JwtSecurityToken
                (
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddHours(3),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigninKey, SecurityAlgorithms.HmacSha256)
                );

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    StatusCode = 200
                });
            }
            return Unauthorized();
        }

        [HttpPost("Logout")]
        [APIKeyAuth]
        public async Task<IActionResult> Logout()
        {
            //await _tokenManager.DeactivateCurrentAsync();
            await _signInManager.SignOutAsync();

            return Ok(new Response { Status = "Success", Message = "User logout successfully" });
        }
    }
}
