using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Messenger_Web_App.Controllers
{
    [Authorize]
    public class MessagesController : Controller
    {
        public IActionResult ChatBox()
        {
            return View();
        }
        public IActionResult ChangeName()
        {
            return View();
        }
        public IActionResult SeeMembers()
        {
            return View();
        }
        public IActionResult AddUsers()
        {
            return View();
        }
        public IActionResult RemoveUsers()
        {
            return View();
        }

        public IActionResult ContactListElement()
        {
            return View();
        }
    }
}
