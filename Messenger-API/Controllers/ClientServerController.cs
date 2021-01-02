using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Messenger_API.Models;
using Messenger_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Messenger_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientServerController : ControllerBase
    {
        private readonly IMessageRepository _message;

        public ClientServerController(IMessageRepository message)
        {
            _message = message;
        }

        /*
        public ActionResult<IEnumerable<Friend>> GetAllFriends(Guid friendId)
        {

        }
        */

        //[Authorize]
        [HttpPost("Accept")]
        public IActionResult AcceptFriendRequest([FromForm] string userName, [FromForm] string friendName)
        {
            var userId = _message.GetIdByName(userName);
            var friendId = _message.GetIdByName(friendName);

            var rez = _message.AcceptFriendRequest(userId, friendId);

            if (rez)
                return Ok("Succes");
            else
                return Ok(userId);
        }

        [HttpPost("Send")]
        public IActionResult SendFriendRequest([FromForm] string userName, [FromForm] string friendName)
        {
            var userId = _message.GetIdByName(userName);
            var friendId = _message.GetIdByName(friendName);

            var rez = _message.SendFriendRequest(userId, friendId);

            if (rez)
                return Ok("Succes");
            else
                return Ok(userId);
        }

        [HttpPost("Remove")]
        public IActionResult RemoveFriend([FromForm] string userName, [FromForm] string friendName)
        {
            var userId = _message.GetIdByName(userName);
            var friendId = _message.GetIdByName(friendName);

            var rez = _message.RemoveFriend(userId, friendId);

            if (rez)
                return Ok("Succes");
            else
                return Ok(userId);
        }

        [HttpPost("AddImageProfile")]
        public IActionResult AddImageProfile([FromForm] ImageProfile imageProfile, [FromForm] List<IFormFile> Image)
        {
            var rez = _message.AddImageProfile(imageProfile, Image);

            if (rez)
                return Ok(rez);
            else
                return Ok("Fail");
        }
    }
}
