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

        static Random Random = new Random();

        static Dictionary<string, List<string>> CurrentUsers = new Dictionary<string, List<string>>();

        static Dictionary<string, List<Conversation>> Conversations = new Dictionary<string, List<Conversation>>();
        
        static Dictionary<string, List<Packet>> Packets = new Dictionary<string, List<Packet>>();

        static Dictionary<string, List<MessageContent>> Messages = new Dictionary<string, List<MessageContent>>();

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
                Console.WriteLine("joined with null username");
                return;
            }

            await AddConnectionId(GetUserId(username), Context.ConnectionId);

            LoadConversationsForUser(GetUserId(username));

            await base.OnConnectedAsync();
        }

        [Authorize]
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string username = Context.GetHttpContext().User.Identity.Name;

            if (string.IsNullOrEmpty(username))
            {
                Console.WriteLine("left with null username");
                return;
            }

            UnloadConversationsForUser(GetUserId(username));

            await RemoveConnectionId(GetUserId(username), Context.ConnectionId);

            await base.OnDisconnectedAsync(exception);
        }

        [Authorize]
        public async Task SendMessage(string conversationId, string message)
        {
            if (string.IsNullOrEmpty(conversationId)) return;

            if (string.IsNullOrEmpty(message)) return;

            var username = Context.User.Identity.Name;

            if (string.IsNullOrEmpty(username)) return;

            var ownConnectionsId = GetConnectionsId(GetUserId(username));

            foreach(var connection in ownConnectionsId)
            {
                await Clients.Client(connection).SendAsync("SendMessage", conversationId, message);
            }

            await Clients.AllExcept(ownConnectionsId).SendAsync("ReceiveMessage", conversationId, message);
        }

        // Conversations

        [Authorize]
        public async Task ListConversations()
        {
            var username = Context.User.Identity.Name;
            var userId = GetUserId(username);

            if (userId == null) return;

            var list = GetAllHubConversations(userId);

            if (list == null) return;

            // here we will order by the last sent message date
            //list = list.OrderBy(x => x.ConversationName).ToList();
            
            await Clients.Caller.SendAsync("ListConversations", list);
        }

        [Authorize]
        public async Task OpenConversation(string conversationId)
        {
            List<Conversation> conversation = GetConversation(conversationId);
            if(conversation == null)
            {
                return;
            }

            var data = new HubChatroomConversation { Id = conversationId };

            data.ConversationName = GetConversationName(conversationId);
            
            await Clients.Caller.SendAsync("EnterConversation", data);
        }

        [Authorize]
        public async Task FindConversation(string conversationName)
        {
            if (string.IsNullOrEmpty(conversationName)) return;

            var userId = GetUserId(Context.User.Identity.Name);

            if(string.IsNullOrEmpty(userId))
            {
                return;
            }

            var foundConversations = Conversations.Values.Where(conversation => conversation.First().UserId.Equals(userId) && conversation.First().ConversationId.Contains(conversationName))
                .Select(c => new HubConversation { ConversationName = c.First().ConversationId, Id = c.First().ConversationId }).ToList();
            Console.WriteLine(foundConversations.Count);
            await Clients.Caller.SendAsync("ListFoundConversations", foundConversations);
        }

        [Authorize]
        public async Task CreateNewConversation()
        {
            var user = messageContext.SmallUsers.FirstOrDefault(u => u.UserName.Equals(Context.User.Identity.Name));

            string userId = user == default ? "" : user.UserId;

            if (userId.Equals("")) return;

            Conversation member = GenerateEmptyConversation(userId);

            AddConversation(new List<Conversation>() { member });

            await Clients.Caller.SendAsync("EnterConversation", new HubChatroomConversation { Id = member.ConversationId, ConversationName = GetConversationName(member.ConversationId)});
        }

        // Helper methods

        List<HubConversation> GetAllHubConversations(string userId)
        {
            if(string.IsNullOrEmpty(userId))
            {
                return null;
            }

            var conversations = GetAllConversations(userId);
            
            if (conversations == null) return null;

            var hubConversations = conversations
                .Select(c => new HubConversation { ConversationName = GetConversationName(c.First().ConversationId), 
                    Id = c.First().ConversationId})
                .ToList();

            return hubConversations;
        }

        List<List<Conversation>> GetAllConversations(string userId)
        {
            if(string.IsNullOrEmpty(userId))
            {
                return null;
            }

            var conversations = (from c in Conversations
                                 where c.Value.FirstOrDefault(
                                     m => m.UserId.Equals(userId))
                                 != default select c.Value).ToList();

            if (conversations.Count == 0) return null;

            return conversations;
        }

        void LoadConversationsForUser(string userId)
        {
            if(string.IsNullOrEmpty(userId))
            {
                return;
            }

            // we are selecting all conversations ids where userId is member
            var conversationsId = (from c in messageContext.Conversations
                                 where c.UserId.Equals(userId)
                                 select c.ConversationId).ToList();

            if(conversationsId.Count == 0)
            {
                return;
            }

            foreach(var id in conversationsId)
            {
                LoadConversation(id);
            }
        }

        void UnloadConversationsForUser(string userId) // unloading all conversations wher the user is the only one member
        {
            // Disabled

            /*if (string.IsNullOrEmpty(userId))
            {
                return;
            }

            // we are selecting all conversations ids where userId is member
            var conversationsId = (from c in messageContext.Conversations
                                   where c.UserId.Equals(userId)
                                   && c.ConversationId.Count() == 1
                                   select c.ConversationId).ToList();

            if (conversationsId.Count == 0)
            {
                return;
            }

            foreach (var id in conversationsId)
            {
                UnloadConversation(id);
            }*/
        }

        void AddUserToConversation(string userId, string conversationId)
        {
            if (!IsConversationIdValid(conversationId)) return;
            if (Conversations[conversationId].FirstOrDefault(m => m.UserId.Equals(userId)) != default) return;

            var member = new Conversation { UserId = userId, ConversationId = conversationId };

            try
            { 
                // we might get an error if we already got an object with the same values
                messageContext.Conversations.Add(member);
            }catch
            {
                // then the object is already there
            }

            Conversations[conversationId].Add(member);
        }

        void AddConversation(List<Conversation> conversation) // Keep in mind that one conversation object is an entity for one member of the conversation, such as a list of conversation objects will make a conversation group
        {
            if (conversation.Count < 1) return;
            if (Conversations.ContainsKey(conversation[0].ConversationId)) return; // then we suppose that we have already added this conversation

            Conversations.Add(conversation[0].ConversationId, conversation);
        }

        List<Conversation> GetConversationWithUser(string userId)
        {
            var caller = messageContext.SmallUsers.FirstOrDefault(u => u.UserName.Equals(Context.User.Identity.Name));

            string callerId = caller == default ? "" : caller.UserId;

            if (userId.Equals("")) return null;

            var conversations = (from e in messageContext.Conversations
                                 where e.UserId.Equals(callerId)
                                 || e.UserId.Equals(userId)
                                 select e).ToList();

            if (conversations.Count < 2) // then the userId is invalid or the callerId was wrong
            {
                return null;
            }else if(conversations.Count == 2 && conversations[0].ConversationId.Equals(conversations[1].ConversationId))
            {
                return conversations;
            }
            
            Dictionary<string, List<Conversation>> possibleConversationIds = new Dictionary<string, List<Conversation>>();

            foreach(var member in conversations)
            {
                if(possibleConversationIds.ContainsKey(member.ConversationId))
                {
                    possibleConversationIds[member.ConversationId].Add(member);
                }else
                {
                    possibleConversationIds.Add(member.ConversationId, new List<Conversation> { member });
                }
            }

            if(possibleConversationIds.Count == 0)
            {
                return null;
            }else if(possibleConversationIds.Count == 1)
            {
                string key = possibleConversationIds.Keys.First();
                if (possibleConversationIds[key].Count == 2) return possibleConversationIds[key];
            } 
            foreach(var key in possibleConversationIds.Keys)
            {
                if (possibleConversationIds[key].Count == 2) return possibleConversationIds[key];
            }
            return null;
        }

        void LoadConversation(string conversationId)
        {
            if(string.IsNullOrEmpty(conversationId))
            {
                return;
            }

            if(!Conversations.ContainsKey(conversationId))
            {
                var conversation = LoadConversationFromContext(conversationId);
                if (conversation == null) return;
                Conversations.Add(conversationId, conversation);
            }
        }

        void UnloadConversation(string conversationId)
        {
            if(string.IsNullOrEmpty(conversationId))
            {
                return;
            }

            if(Conversations.ContainsKey(conversationId))
            {
                if (Conversations[conversationId].Count == 1) Conversations.Remove(conversationId);
            }
        }

        List<Conversation> GetConversation(string conversationId)
        {
            if(string.IsNullOrEmpty(conversationId))
            {
                return null;
            }
            if(!Conversations.ContainsKey(conversationId)) // if we don't have loaded the conversation
            { // then we search it in the context
                if(messageContext.Conversations.FirstOrDefault(c => c.ConversationId.Equals(conversationId)) != default)
                {
                    return messageContext.Conversations.Where(c => c.ConversationId.Equals(conversationId)).ToList();
                }
            }
            else // otherwise we return it from the dictionary
            {
                return Conversations[conversationId];
            }
            return null;
        }

        List<string> GetConversationMembersIds(string conversationId)
        {
            if (!IsConversationIdValid(conversationId))
            {
                return null;
            }

            List<Conversation> conversation = GetConversation(conversationId);

            if (conversation == null || conversation.Count == 0)
            {
                return null;
            }

            List<string> ids = conversation.Select(c => c.UserId).ToList();

            return ids;
        }

        bool IsConversationIdValid(string conversationId)
        {
            return Conversations.ContainsKey(conversationId) || messageContext.Conversations.FirstOrDefault(c => c.ConversationId.Equals(conversationId)) != default;
        }

        List<Conversation> LoadConversationFromContext(string conversationId)
        {
            if(string.IsNullOrEmpty(conversationId))
            {
                return null;
            }

            var conversation = (from c in messageContext.Conversations
                                where c.ConversationId.Equals(conversationId)
                                select c).ToList();

            if(conversation.Count == 0)
            {
                return null;
            }

            return conversation;
        }

        bool AddConversationToContext(Conversation conversation)
        {
            if (messageContext.Conversations.Find(conversation.ConversationId, conversation.UserId) == null)
            {
                messageContext.Conversations.Add(conversation);
                messageContext.SaveChanges();
                return true;
            }
            return false;
        }

        Conversation GenerateEmptyConversation(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return null;

            var user = messageContext.SmallUsers.Find(userId);

            if (user == null) return null;

            string conversationId = GenerateConversationId();
            while (messageContext.Conversations.FirstOrDefault(c => c.ConversationId.Equals(conversationId)) != default)
            {
                // we keep generating until we get a non used id
                conversationId = GenerateConversationId();
            }

            var member = new Conversation
            {
                ConversationId = conversationId,
                UserId = userId,
                IsAdmin = true,
                SmallUser = user
            };

            if (!AddConversationToContext(member))
            {
                return null;
            }

            SetConversationName(conversationId, "new conversation");

            return member;
        }

        // Conversation name

        void SetConversationName(string conversationId, string conversationName)
        {
            if (string.IsNullOrEmpty(conversationId)) return;
            if (string.IsNullOrEmpty(conversationName)) return;

            var details = messageContext.ConversationDetails.FirstOrDefault(d => d.ConversationId.Equals(conversationId));
            
            if(details == default) // then we have no name reference on the conversation details table
            {
                details = new ConversationDetail
                {
                    Conversation = GetConversation(conversationId).First(),
                    ConversationId = conversationId,
                    ConversationName = conversationName
                };

                messageContext.ConversationDetails.Add(details);
            }
            else
            {
                details.ConversationName = conversationName;
            }

            messageContext.SaveChanges();
        }

        string GetConversationName(string conversationId)
        {
            if (string.IsNullOrEmpty(conversationId)) return "";

            var details = messageContext.ConversationDetails.FirstOrDefault(d => d.ConversationId.Equals(conversationId));

            if (details == default) return "";

            return details.ConversationName;
        }

        // (Message) Packets

        void AddMessageToPacket(string packetId, string message)
        {
            if (string.IsNullOrEmpty(packetId)) return;
            if (string.IsNullOrEmpty(message)) return;

            
        }

        void AddPacketToContext(string packetId, string conversationId)
        {
            if (string.IsNullOrEmpty(packetId)) return;
            if (string.IsNullOrEmpty(conversationId)) return;

            if (!Packets.ContainsKey(conversationId)) return;

            var packet = Packets[conversationId].FirstOrDefault(p => p.PacketId.Equals(packetId));

            if (packet == default) return;

            if (messageContext.Packets.Find(packetId) == null)
            {
                // then we 

            }
        }

        // Messages
        
        void StoreMessage(string conversationId, string message)
        {
            if (string.IsNullOrEmpty(conversationId)) return;
            if (string.IsNullOrEmpty(message)) return;


        }

        string GetUsername(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return "";
            }

            var user = messageContext.SmallUsers.Find(userId);

            return user == null ? "" : user.UserName;
        }

        string GetUserId(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return "";
            }

            var user = messageContext.SmallUsers.FirstOrDefault(u => u.UserName.Equals(username));

            return user == default ? "" : user.UserId;
        }

        bool IsUserActive(string username)
        {
            return CurrentUsers.ContainsKey(username);
        }

        // Connection ids

        List<string> GetConnectionsId(string userId)
        {
            if (!CurrentUsers.ContainsKey(userId)) return null;

            return CurrentUsers[userId];
        }

        async Task AddConnectionId(string userId, string connectionId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);

            if(CurrentUsers.ContainsKey(userId))
            {
                if (!CurrentUsers[userId].Contains(connectionId)) { CurrentUsers[userId].Add(connectionId); return; }
            }

            CurrentUsers.Add(userId, new List<string> { connectionId });
        }

        async Task RemoveConnectionId(string userId, string connectionId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);

            if (CurrentUsers.ContainsKey(userId))
            {
                if (CurrentUsers[userId].Contains(connectionId)) CurrentUsers[userId].Remove(connectionId);
                if (CurrentUsers[userId].Count == 0) CurrentUsers.Remove(userId);
                
            }
        }

        // Generators

        string GenerateConversationId()
        {
            return GenerateId(400);
        }

        string GenerateId(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[Random.Next(s.Length)]).ToArray());
        }

        // Hub Class Structures

        class HubConversation
        {
            public string Id { get; set; }
            public string ConversationName { get; set; }
        }

        class HubListConversation : HubConversation
        {
            public HubMessage LastMessage { get; set; }
        }

        class HubChatroomConversation : HubConversation
        {
            public List<string> Members { get; set; }
        }

        class HubMessagePacket
        {
            public string ConversationId { get; set; }
            public string PacketId { get; set; }
            public int PacketNumber { get; set; }
            public List<HubMessage> Messages { get; set; }
        }

        class HubMessage
        {
            public string Sender { get; set; }
            public DateTime SentData { get; set; }
        }
    }
}
