using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Messenger_Web_App.Models;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System.Net.Http;

namespace Messenger_Web_App.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<Login> userManager;

        private readonly ILogger<HomeController> _logger;
        private object httpClient;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        //[Authorize]
        public IActionResult Index()
        {
            if(HttpContext.User.Identity.IsAuthenticated)
            {
                var model = new Login() { Username = HttpContext.User.Identity.Name };
                return View(model);
            }
            
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /*public async Task<IActionResult> Test()
        {
            using (var response = await httpClient.GetAsync(Startup.Constants.API_Address+"/authentication/api/Test", new FormUrlEncodedContent(body)))
            {
                string apiResponse = await response.Content.ReadAsStringAsync();
                //return RedirectToAction("Test", "Profile", apiResponse);  // use this route in order to read the response
                
            }
            return Ok("no");
        }*/
    }
}
