using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Messenger_Web_App.Models;
using Microsoft.AspNetCore.Identity;

namespace Messenger_Web_App.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<Login> userManager;

        private readonly ILogger<HomeController> _logger;

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
    }
}
