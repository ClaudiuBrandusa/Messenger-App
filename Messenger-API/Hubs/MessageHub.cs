using Messenger_API.Authentication;
using Messenger_API.Data;
using Messenger_API.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Messenger_API.Hubs
{
    public class MessageHub : Hub
    {
        MessageContext messageContext;
        UserManager<ApplicationUser> userManager;

        static List<string> CurrentUsers = new List<string>();

        public MessageHub(MessageContext messageContext, UserManager<ApplicationUser> userManager)
        {
            this.messageContext = messageContext;
            this.userManager = userManager;
        }

        [Authorize]
        public override async Task OnConnectedAsync()
        {
            string username = Context.GetHttpContext().User.Identity.Name;

            if(string.IsNullOrEmpty(username))
            {
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, username);

            CurrentUsers.Add(username);

            await base.OnConnectedAsync();
        }

        [Authorize]
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string username = Context.GetHttpContext().User.Identity.Name;

            if (string.IsNullOrEmpty(username))
            {
                return;
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, Context.GetHttpContext().User.Identity.Name);

            if(CurrentUsers.Contains(Context.GetHttpContext().User.Identity.Name))
            {
                CurrentUsers.Remove(Context.GetHttpContext().User.Identity.Name);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string user, string message)
        {
            if(messageContext.SmallUsers.FirstOrDefault(u => u.UserName.Equals(user)) == default)
            {
                return; // then the user is not authorized
            }

            await Clients.Client(Context.ConnectionId).SendAsync("SendMessage", message);
            await Clients.AllExcept(Context.ConnectionId).SendAsync("ReceiveMessage", user, message);
        }

        // Conversations

        [Authorize]
        public async Task FindConversation(string conversationName)
        {
            // Find conversation

            // Send conversation id
            await Clients.Caller.SendAsync(conversationName);
        }
    }
}
