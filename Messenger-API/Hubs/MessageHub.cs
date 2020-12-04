using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Messenger_API.Hubs
{
    public class MessageHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.Client(Context.ConnectionId).SendAsync("SendMessage", message);
            await Clients.AllExcept(Context.ConnectionId).SendAsync("ReceiveMessage", user, message);
        }
    }
}
