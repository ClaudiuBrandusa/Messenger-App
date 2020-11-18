using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Messenger_API.Authentication;
using Messenger_API.Data;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Messenger_Web_App.Controllers
{
    public class ProfileController : Controller
    {
        public ViewResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(Register register)
        {
            Register receivedRegister = new Register();
            using (var httpClient = new HttpClient())
            {
                StringContent content = new StringContent(JsonConvert.SerializeObject(register), Encoding.UTF8, "application/json");

                using (var response = await httpClient.PostAsync("https://localhost:44338/api/authentication", content))
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
    }
}
