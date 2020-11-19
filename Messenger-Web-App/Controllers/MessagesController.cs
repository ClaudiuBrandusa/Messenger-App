using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Messenger_Web_App.Controllers
{
    public class MessagesController : Controller
    {
        public IActionResult ChatBox()
        {
            return View();
        }

        public IActionResult ChatBox2()
        {
            return View();
        }

        public IActionResult ChatBoxSettings()
        {
            return View();
        }
    }
}
