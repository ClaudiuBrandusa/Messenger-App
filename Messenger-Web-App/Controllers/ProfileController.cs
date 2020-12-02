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
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("ApiKey", "ThisMySecretKey123");

                StringContent content = new StringContent(JsonConvert.SerializeObject(register), Encoding.UTF8, "application/json");

                using (var response = await httpClient.PostAsync("http://localhost:49499/api/Authentication/Register", content))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    receivedRegister = JsonConvert.DeserializeObject<Register>(apiResponse);
                }
            }
            return View(receivedRegister);
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
