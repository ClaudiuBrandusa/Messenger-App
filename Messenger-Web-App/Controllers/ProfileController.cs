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

namespace Messenger_Web_App.Controllers
{
    //[Authorize]
    public class ProfileController : Controller
    {


        public ViewResult Register() => View();

        [HttpPost]
        //[AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Register register)
        {
            Register receivedRegister = new Register();
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("ApiKey", "ThisMySecretKey123");

                string json = JsonConvert.SerializeObject(register);

                //StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                Dictionary<string, string> body = JsonConvert.DeserializeObject<Dictionary<string,string>>(json);
                using (var response = await httpClient.PostAsync(Startup.Constants.Register_Endpoint, new FormUrlEncodedContent(body)))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    //return RedirectToAction("Test", "Profile", apiResponse);  // use this route in order to read the response
                    receivedRegister = JsonConvert.DeserializeObject<Register>(apiResponse);
                }
            }
            return View(receivedRegister);
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
            Login receivedLogin = new Login();
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("ApiKey", "ThisMySecretKey123");

                string json = JsonConvert.SerializeObject(login);

                //StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                Dictionary<string, string> body = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                using (var response = await httpClient.PostAsync(Startup.Constants.Login_Endpoint, new FormUrlEncodedContent(body)))
                {
                    //return Redirect(response.StatusCode.ToString());
                    if (response.StatusCode.ToString().Equals("OK"))
                    {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, login.Username),
                        };
                        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        ClaimsPrincipal principal = new ClaimsPrincipal(identity);
                        AuthenticationProperties authProperties = new AuthenticationProperties 
                        {
                            AllowRefresh = true,
                            ExpiresUtc = DateTimeOffset.Now.AddMinutes(30),
                            IsPersistent = true,
                        };  
                        await HttpContext.SignInAsync(principal, authProperties); 
                        return Redirect("~/");
                    }
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    receivedLogin = JsonConvert.DeserializeObject<Login>(apiResponse);
                    return View(receivedLogin);               
                }
            }    
        }

        //public ViewResult Logout() => View();

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
                    await HttpContext.SignOutAsync();
                    return Redirect("~/");
                }
            }
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
    }
}
