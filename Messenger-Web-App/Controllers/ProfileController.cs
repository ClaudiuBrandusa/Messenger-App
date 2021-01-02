using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Messenger_API.Data;
using Messenger_Web_App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;

namespace Messenger_Web_App.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IConfiguration _configuration;

        public ProfileController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ViewResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Register register)
        {
            if (ModelState.IsValid)
            {
                Register receivedRegister = new Register();
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("ApiKey", "ThisMySecretKey123");

                    string json = JsonConvert.SerializeObject(register);

                    Dictionary<string, string> body = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    using (var response = await httpClient.PostAsync(Startup.Constants.Register_Endpoint, new FormUrlEncodedContent(body)))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        Dictionary<string, string> dictionaryResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(apiResponse);

                        if (response.StatusCode.ToString().Equals("OK"))
                        {
                            var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.Name, register.UserName)
                            };
                            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                            ClaimsPrincipal principal = new ClaimsPrincipal(identity);
                            var a = new JwtSecurityTokenHandler().ReadToken(dictionaryResponse["token"]);
                            AuthenticationProperties authProperties = new AuthenticationProperties
                            {
                                AllowRefresh = true,
                                ExpiresUtc = DateTimeOffset.Now.AddSeconds(Math.Max((a.ValidTo - DateTime.UtcNow).TotalSeconds - 5, 5)),
                                IsPersistent = true
                            };
                            await HttpContext.SignInAsync(principal, authProperties);
                            if (HttpContext.Request.Cookies.ContainsKey("access_token"))
                            {
                                HttpContext.Response.Cookies.Delete("access_token");
                            }
                            if (HttpContext.Request.Cookies.ContainsKey("refresh_token"))
                            {
                                HttpContext.Response.Cookies.Delete("refresh_token");
                            }
                            HttpContext.Response.Cookies.Append("access_token", dictionaryResponse["token"]);
                            HttpContext.Response.Cookies.Append("refresh_token", dictionaryResponse["refreshToken"]);

                            return Redirect("~/");
                        }

                        receivedRegister = JsonConvert.DeserializeObject<Register>(apiResponse);
                    }
                }
                return View(receivedRegister);
            }
            return View(register);
        }

        public IActionResult Test()
        {
            return View();
        }

        public ViewResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(Login login)
        {
            if(ModelState.IsValid)
            { 
                Login receivedLogin = new Login();
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("ApiKey", "ThisMySecretKey123");

                    string json = JsonConvert.SerializeObject(login);

                    Dictionary<string, string> body = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    using (var response = await httpClient.PostAsync(Startup.Constants.Login_Endpoint, new FormUrlEncodedContent(body)))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        Dictionary<string, string> dictionaryResponse = JsonConvert.DeserializeObject<Dictionary<string,string>>(apiResponse);
                    
                        if (response.StatusCode.ToString().Equals("OK"))
                        {
                            ClaimsPrincipal principal = GetPrincipalFromExpiredToken(dictionaryResponse["token"]);
                            var a = new JwtSecurityTokenHandler().ReadToken(dictionaryResponse["token"]);
                            AuthenticationProperties authProperties = new AuthenticationProperties 
                            {
                                AllowRefresh = true,
                                ExpiresUtc = DateTimeOffset.Now.AddSeconds(Math.Max((a.ValidTo - DateTime.UtcNow).TotalSeconds-5, 5)), // clamping the value at 5 seconds
                                IsPersistent = true
                            };  
                            await HttpContext.SignInAsync(principal, authProperties);
                            if (HttpContext.Request.Cookies.ContainsKey("access_token"))
                            {
                                HttpContext.Response.Cookies.Delete("access_token");
                            }
                            HttpContext.Response.Cookies.Append("access_token", dictionaryResponse["token"]);
                            HttpContext.Response.Cookies.Append("refresh_token", dictionaryResponse["refreshToken"]);

                            return Redirect("~/");
                        }
<<<<<<< HEAD
                        receivedLogin = JsonConvert.DeserializeObject<Login>(apiResponse);
                        return View(receivedLogin);               
=======
                        HttpContext.Response.Cookies.Append("access_token", dictionaryResponse["token"]);
                        HttpContext.Response.Cookies.Append("refresh_token", dictionaryResponse["refreshToken"]);


                        return Redirect("~/");
>>>>>>> d2f14f9562207a66caa524daf61d32bf28e6d966
                    }
                }
            }
            return View(login);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("ApiKey", "ThisMySecretKey123");

                using (var response = await httpClient.PostAsync(Startup.Constants.Logout_Endpoint, null))
                {
                    HttpContext.Response.Cookies.Delete("access_token");
                    HttpContext.Response.Cookies.Delete("refresh_token");
                    await HttpContext.SignOutAsync();
                    return Redirect("~/");
                }
            }
        }

        [HttpPost("Refresh")]
        public async Task<IActionResult> RefreshToken([FromForm] string token, [FromForm] string refreshToken)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("ApiKey", "ThisMySecretKey123");

                Dictionary<string, string> body = new Dictionary<string, string>();

                body.Add("token", token);
                body.Add("refreshToken", refreshToken);

                using (var response = await httpClient.PostAsync(Startup.Constants.API_Address + "/api/authentication/refresh", new FormUrlEncodedContent(body)))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();

                    Dictionary<string, string> dictionaryResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(apiResponse);

                    if(!dictionaryResponse.ContainsKey("statusCode"))
                    {
                        Console.WriteLine("no status code");
                        return BadRequest();
                    }

                    int statusCode = 0;

                    try
                    {
                        statusCode = Int32.Parse(dictionaryResponse["statusCode"]);
                    }catch
                    {
                        Console.WriteLine("exception");
                        return BadRequest();
                    }
                    if(statusCode == 422)
                    {
                        Console.WriteLine(422);
                        return BadRequest();
                    }

                    if (!dictionaryResponse.ContainsKey("token") || !dictionaryResponse.ContainsKey("refreshToken"))
                    {
                        Console.WriteLine("Empty dictionary");
                        return BadRequest();
                    }

                    Console.WriteLine(Uri.EscapeUriString(dictionaryResponse["refreshToken"]));

                    // Updating the user cookies if we are able, otherwise it means that we had accessed this route from the outside of the web app

                    HttpContext?.Response?.Cookies?.Delete("access_token");
                    HttpContext?.Response?.Cookies?.Delete("refresh_token");

                    HttpContext?.Response?.Cookies?.Append("access_token", dictionaryResponse["token"]);
                    HttpContext?.Response?.Cookies?.Append("refresh_token", dictionaryResponse["refreshToken"]);

                    return Ok(new { StatusCode = statusCode, token = dictionaryResponse["token"], refreshToken = dictionaryResponse["refreshToken"] });
                }
            }
        }

        [HttpPost("GetTokenExpirationTime")]
        public string GetTokenExpirationTime([FromForm] string token, [FromForm] string refreshToken)
        {
            if(string.IsNullOrEmpty(token) || string.IsNullOrEmpty(refreshToken))
            {
                return "invalid input";
            }

            double time = GetTokenExpirationTime(token);

            if(time == 0)
            {
                return "invalid token";
            }
            return time.ToString();
        }
        public IActionResult AddContact()
        {
            return View();
        }

        public IActionResult Account()
        {
            return View();
        }

        public IActionResult EditAccount()
        {
            return View();
        }

        // Helpers methods

        ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:ApiKey"])),
                ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            if(!tokenHandler.CanReadToken(token))
            {
                return null;
            }
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            { 
                Console.WriteLine("Invalid token");
                return null;
            }

            return principal;
        }

        double GetTokenExpirationTime(string token)
        {
            if(!IsTokenReadable(token))
            {
                return 0;
            }
            var a = new JwtSecurityTokenHandler().ReadToken(token);

            return (a.ValidTo - DateTime.UtcNow).TotalSeconds;
        }

        bool IsTokenReadable(string token)
        {
            return new JwtSecurityTokenHandler().CanReadToken(token);
        }
    }
}
