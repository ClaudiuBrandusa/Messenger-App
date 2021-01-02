using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Messenger_API.Authentication;
using Messenger_API.Data;
using Messenger_API.Filters;
using Messenger_API.Models;
using Messenger_API.Services;
using Messenger_API.Tokens;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
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
        private readonly ITokenRepository tokenRepository;
        private readonly ITokenHandler tokenHandler;
        private readonly IUserRepository userRepository;

        public AuthenticationController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration,  
                                        SignInManager<ApplicationUser> signInManager, MessageContext messageContext, ITokenRepository tokenRepository,
                                        ITokenHandler tokenHandler, IUserRepository userRepository)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.messageContext = messageContext;
            this.tokenRepository = tokenRepository;
            _configuration = configuration;
            _signInManager = signInManager;
            this.tokenHandler = tokenHandler;
            this.userRepository = userRepository;
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

            var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name,user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

            messageContext.SmallUsers.Add(new Models.SmallUser { UserId = user.SecurityStamp, UserName = user.UserName });
            await messageContext.SaveChangesAsync();

            var token = tokenHandler.GenerateToken(authClaims);

            return Ok(new
            {
                token = tokenHandler.WriteToken(token),
                refreshToken = tokenHandler.GenerateRefreshToken(user.Id).Token,
                expirationTime = _configuration["JWT:ExpirationTime"],
                StatusCode = 200
            });
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

                var token = tokenHandler.GenerateToken(authClaims);

                return Ok(new
                {
                    token = tokenHandler.WriteToken(token),
                    refreshToken = tokenHandler.GenerateRefreshToken(user.Id).Token,
                    expirationTime = _configuration["JWT:ExpirationTime"],
                    StatusCode = 200
                });
            }
            return Unauthorized();
        }

        [Authorize]
        [HttpPost("Logout")]
        [APIKeyAuth]
        public async Task<IActionResult> Logout()
        {
            var userId = userRepository.GetIdByUsername(User.Identity.Name);

            await _signInManager.SignOutAsync();

            return Ok(new Response { Status = "Success", Message = "User logout successfully" });
        }


        // Refresh JWT Token
        [HttpPost("Refresh")]
        [APIKeyAuth]
        public IActionResult Refresh([FromForm]string token, [FromForm]string refreshToken)
        {
            var principal = tokenHandler.GetPrincipalFromExpiredToken(token);
            if (principal == null || principal == default)
            {
                return new ObjectResult(new
                {
                    StatusCode = 422
                });
            }
            var username = principal.Identity.Name;
            var userId = userRepository.GetIdByUsername(username);
            if(!tokenHandler.IsTokenAvailableForUserId(userId, refreshToken))
            {
                Console.WriteLine("Invalid refresh token Here");
                return new ObjectResult(new
                {
                    StatusCode = 422
                });
            }

            tokenRepository.RemoveRefreshToken(refreshToken);

            var newJwtToken = tokenHandler.GenerateToken(principal.Claims);
            var newRefreshToken = tokenHandler.GenerateRefreshToken(userId).Token;
            Console.WriteLine("new reftoken: "+newRefreshToken);
            return new ObjectResult(new { 
                StatusCode = 200,
                token = tokenHandler.WriteToken(newJwtToken),
                refreshToken = newRefreshToken
            });
        }

    }
}
