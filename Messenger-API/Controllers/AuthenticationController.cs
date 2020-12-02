using System;
using System.Threading.Tasks;
using Messenger_API.Authentication;
using Messenger_API.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Messenger_API.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]

    public class AuthenticationController : Controller 
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration _configuration;

        public AuthenticationController(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            this.userManager = userManager;
            _configuration = configuration;
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
            return Ok(new Response { Status = "Success", Message = "User created successfully" });
        }
    }
}
