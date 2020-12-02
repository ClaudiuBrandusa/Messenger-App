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
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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

                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                Dictionary<string, string> body = JsonConvert.DeserializeObject<Dictionary<string,string>>(json);
                using (var response = await httpClient.PostAsync("http://localhost:49499/api/authentication/register", new FormUrlEncodedContent(body)))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    //return RedirectToAction("Test", "Profile", apiResponse);  // use this route in order to read the response
                    receivedRegister = JsonConvert.DeserializeObject<Register>(apiResponse);
                }
            }
            return View(receivedRegister);
        }

        public IActionResult Test(string content)
        {
            return View(content);
        }

        public IActionResult Login()
        {
            return View();
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
