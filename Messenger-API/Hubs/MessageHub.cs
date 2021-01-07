using Messenger_API.Authentication;
using Messenger_API.Data;
using Messenger_API.Entities;
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

            bool firstInteraction = IsConversationEmpty(conversationId);

            StoreMessage(conversationId, message);

            var data = new HubListConversation
            {
                Id = conversationId,
                ConversationName = GetConversationName(conversationId)
            };

            Console.WriteLine(data.ConversationName);

            foreach(var connection in ownConnectionsId)
            {
                await Clients.Client(connection).SendAsync("SendMessage", conversationId, message);
                if (firstInteraction) await Clients.Client(connection).SendAsync("AddConversationInList", data);
            }

            List<string> receiverIds = GetConversationMembersIds(conversationId);

            foreach(var receiver in receiverIds)
            {
                if(CurrentUsers.ContainsKey(receiver) && !receiver.Equals(GetUserId(username)))
                {
                    await Clients.Group(receiver).SendAsync("ReceiveMessage", conversationId, message);
                    if (firstInteraction)
                    {
                        data.ConversationName = GetConversationName(conversationId, receiver);
                        await Clients.Group(receiver).SendAsync("AddConversationInList", data);
                    }
                }
            }
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

            var data = new HubChatroomConversation { Id = conversationId, ConversationName = GetConversationName(conversationId), Messages = GetMessageHistory(conversationId) };

            data.ConversationName = GetConversationName(conversationId);
            
            await Clients.Caller.SendAsync("EnterConversation", data);
            if (!IsConversationEmpty(conversationId)) await Clients.Caller.SendAsync("AddConversationInList", data);
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
            
            // finding conversations

            var possibleConversationIds = messageContext.ConversationMembers.Where(m =>
                m.UserId.Equals(userId)).Select(m => m.ConversationId).ToList();

            List<HubConversation> conversationResults = new List<HubConversation>();

            foreach (var id in possibleConversationIds)
            {
                string name = GetConversationName(id);
                
                if (!name.ToLower().StartsWith(conversationName)) continue;

                conversationResults.Add(new HubConversation { ConversationName = name, Id = id });
            }

            // finding contacts

            var contacts = messageContext.SmallUsers.Where(u => u.UserName.ToLower().StartsWith(conversationName)).ToList();

            var contactsConversations = new List<HubConversation>();

            foreach(var contact in contacts)
            {
                var conversation = GetConversationWithUser(contact.UserId, false); // false means that we are not looking for group conversations

                if(contact.UserId.Equals(userId) && conversation != null)
                {
                    var aux = GetSelfConversation(userId);
                    if (aux == null)
                    {
                        conversation = null;
                    }
                    else 
                    {
                        conversation = new List<ConversationMember>() { aux };
                    }
                }

                Conversation tmp = null;

                if (conversation == null)
                {
                    tmp = GenerateEmptyConversation(userId);
                    if (tmp == null) continue;
                    if(!contact.UserId.Equals(userId)) AddUserToConversation(contact.UserId, tmp.ConversationId);
                }
                else
                {
                    contactsConversations.Add(new HubConversation { ConversationName = GetUsername(contact.UserId), Id = conversation.First().ConversationId });
                    continue;
                }

                contactsConversations.Add(new HubConversation { ConversationName = GetUsername(contact.UserId), Id = tmp.ConversationId });
            }

            await Clients.Caller.SendAsync("ListFoundConversations", conversationResults, contactsConversations);
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

        List<HubConversation> GetAllHubConversations(string userId, bool empty=false)
        {
            if(string.IsNullOrEmpty(userId))
            {
                return null;
            }

            var conversations = GetAllConversations(userId, empty);
            
            if (conversations == null) return null;

            var hubConversations = conversations
                .Select(c => new HubConversation { ConversationName = GetConversationName(c.ConversationId), 
                    Id = c.ConversationId})
                .ToList();

            return hubConversations;
        }

        List<ConversationMember> GetAllConversations(string userId, bool allowEmptyOnes=false)
        {
            if(string.IsNullOrEmpty(userId))
            {
                return null;
            }

            var conversations = messageContext.ConversationMembers.Where(c => c.UserId.Equals(userId)).ToList();

            if (!allowEmptyOnes)
            {
                conversations = conversations.Where(c => !IsConversationEmpty(c.ConversationId)).ToList();
            }

            if (conversations.Count == 0) return null;

            return conversations;
        }

        void LoadConversationsForUser(string userId)
        {
            // Disabled

            /*if(string.IsNullOrEmpty(userId))
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
            }*/
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

        void AddUserToConversation(string userId, string conversationId, bool isGroup = false)
        {
            if (!IsConversationIdValid(conversationId)) return;
            if (messageContext.ConversationMembers.FirstOrDefault(m => m.UserId.Equals(userId) && m.ConversationId.Equals(conversationId)) != default) return;

            var member = new ConversationMember { UserId = userId, ConversationId = conversationId, IsAdmin = !isGroup };

            try
            { 
                // we might get an error if we already got an object with the same values
                messageContext.ConversationMembers.Add(member);
                messageContext.SaveChanges();
            }catch
            {
                // then the object is already there
            }
        }

        bool AddUserToConversation(ConversationMember member)
        {
            if (member == null || member == default) return false;
            if (!IsConversationIdValid(member.ConversationId)) return false;
            if (messageContext.ConversationMembers.FirstOrDefault(m => m.UserId.Equals(member.UserId) && m.ConversationId.Equals(member.ConversationId)) != default) return false;

            try
            {
                // we might get an error if we already got an object with the same values
                messageContext.ConversationMembers.Add(member);
                messageContext.SaveChanges();
            }
            catch
            {
                // then the object is already there
            }
            return true;
        }

        void AddConversation(List<Conversation> conversation) // Keep in mind that one conversation object is an entity for one member of the conversation, such as a list of conversation objects will make a conversation group
        {
            if (conversation.Count < 1) return;
            if (Conversations.ContainsKey(conversation[0].ConversationId)) return; // then we suppose that we have already added this conversation

            Conversations.Add(conversation[0].ConversationId, conversation);
        }

        List<ConversationMember> GetConversationWithUser(string userId, bool allowGroups=true)
        {
            var caller = messageContext.SmallUsers.FirstOrDefault(u => u.UserName.Equals(Context.User.Identity.Name));

            string callerId = caller == default ? "" : caller.UserId;

            if (userId.Equals("")) return null;

            var possibleConversations = (from e in messageContext.ConversationMembers
                                         where e.UserId.Equals(callerId)
                                         || e.UserId.Equals(userId)
                                         select e).ToList();

            if (possibleConversations.Count < 1) // then the userId is invalid or the callerId was wrong
            {
                return null;
            }
            if(!allowGroups)
            {
                var list = new List<ConversationMember>();

                foreach (var conversation in possibleConversations)
                {
                    if (!IsGroupConversation(conversation.ConversationId))
                    {
                        list.Add(conversation);
                    }
                }
                if (list.Count == 0) return null;
                possibleConversations = list;
            }

            else if (possibleConversations.Count == 2 && possibleConversations[0].ConversationId.Equals(possibleConversations[1].ConversationId) && !IsGroupConversation(possibleConversations[0].ConversationId))
            {
                return possibleConversations;
            }

            Dictionary<string, List<ConversationMember>> possibleConversationIds = new Dictionary<string, List<ConversationMember>>();

            foreach (var member in possibleConversations)
            {
                if (IsGroupConversation(member.ConversationId)) continue;

                if (possibleConversationIds.ContainsKey(member.ConversationId))
                {
                    possibleConversationIds[member.ConversationId].Add(member);
                }
                else
                {
                    possibleConversationIds.Add(member.ConversationId, new List<ConversationMember> { member });
                }
            }

            if (possibleConversationIds.Count == 0)
            {
                return null;
            }

            if (userId.Equals(callerId))
            {
                foreach(var conversation in possibleConversationIds.Keys)
                {
                    var member = possibleConversationIds[conversation].FirstOrDefault(c => c.UserId.Equals(userId));
                    if(member != default)
                    {
                        return new List<ConversationMember> { member };
                    }
                } 
            }

            else if (possibleConversationIds.Count == 1)
            {
                string key = possibleConversationIds.Keys.First();
                if (possibleConversationIds[key].Count == 2) return possibleConversationIds[key];
            }

            foreach (var key in possibleConversationIds.Keys)
            {
                if (possibleConversationIds[key].Count == 2) return possibleConversationIds[key];
            }
            return null;
        }

        ConversationMember GetSelfConversation(string userId) // returns the non group conversation with only the user as the member
        {
            if (string.IsNullOrEmpty(userId)) return null;

            var possibleConversations = messageContext.ConversationMembers.Where(m => m.UserId.Equals(userId)).ToList();

            if (possibleConversations.Count == 0) return null;

            foreach(var conversation in possibleConversations)
            {
                if(GetNumberOfMembersInConversation(conversation.ConversationId) == 1)
                {
                    return conversation;
                }
            }
            return null;
        }

        List<Conversation> GetConversation(string conversationId)
        {
            if(string.IsNullOrEmpty(conversationId))
            {
                return null;
            }
            if(messageContext.Conversations.FirstOrDefault(c => c.ConversationId.Equals(conversationId)) != default)
            {
                return messageContext.Conversations.Where(c => c.ConversationId.Equals(conversationId)).ToList();
            }
            return null;
        }

        List<string> GetConversationMembersIds(string conversationId)
        {
            if (!IsConversationIdValid(conversationId))
            {
                return null;
            }

            List<ConversationMember> conversation = (from m in messageContext.ConversationMembers
                                                     where m.ConversationId.Equals(conversationId)
                                                     select m).ToList();

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

        bool IsConversationEmpty(string conversationId)
        {
            return !Messages.ContainsKey(conversationId) || Messages[conversationId].Count == 0;
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
            //if (messageContext.Conversations.Find(conversation.ConversationId, conversation.UserId) == null)
            if (messageContext.Conversations.Find(conversation.ConversationId) == null)
            {
                messageContext.Conversations.Add(conversation);

                messageContext.SaveChanges();
                return true;
            }
            return false;
        }

        Conversation GenerateEmptyConversation(string userId, bool isGroup = false)
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

            var conversation = new Conversation
            {
                ConversationId = conversationId
            };

            var member = new ConversationMember
            {
                ConversationId = conversationId,
                UserId = userId,
                IsAdmin = true,
                SmallUser = user
            };

            if (!AddConversationToContext(conversation))
            {
                return null;
            }

            if(!AddUserToConversation(member))
            {
                return null;
            }

            SetConversationName(conversationId, "new conversation");

            return conversation;
        }

        // Conversation member

        bool IsUserMemberInConversation(string conversationId, string userId)
        {
            return GetConversationMemberData(conversationId, userId) != null;
        }

        int GetNumberOfMembersInConversation(string conversationId)
        {
            return messageContext.ConversationMembers.Where(m => m.ConversationId.Equals(conversationId)).ToList().Count;
        }

        // null means that either the user is not a member of the conversation or the conversation was not found
        ConversationMember GetConversationMemberData(string conversationId, string userId)
        {
            if (string.IsNullOrEmpty(conversationId)) return null;
            if (string.IsNullOrEmpty(userId)) return null;

            var member = messageContext.ConversationMembers.Find(conversationId, userId);

            if (member == null) return null; // member not found

            return member;
        }

        bool IsGroupConversation(string conversationId)
        {
            if (string.IsNullOrEmpty(conversationId)) return false;

            var conversation = messageContext.ConversationDetails.FirstOrDefault(c => c.ConversationId.Equals(conversationId));

            if (conversation == default) return false;

            return conversation.isGroup;
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

        string GetConversationName(string conversationId, string userId="")
        {
            if (string.IsNullOrEmpty(conversationId)) return "";

            var details = messageContext.ConversationDetails.FirstOrDefault(d => d.ConversationId.Equals(conversationId));

            if (details == default) return "";

            string name = "";

            if (IsGroupConversation(conversationId))
            {
                name = details.ConversationName;
            }
            else
            {
                var user = messageContext.ConversationMembers.FirstOrDefault(c => c.ConversationId.Equals(conversationId) && !c.UserId.Equals(userId.Equals("")?GetUserId(Context.User.Identity.Name):userId));
                if(user == default) // then there is no other user in the conversation
                {
                    // so we will give the caller's username
                    name = Context.User.Identity.Name;
                }
                else
                {
                    name = GetUsername(user.UserId);
                }
            }

            return name;
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
        
        void StoreMessage(string conversationId, string content)
        {
            if (string.IsNullOrEmpty(conversationId) || !IsConversationIdValid(conversationId)) return;
            if (string.IsNullOrEmpty(content)) return;

            string userId = GetUserId(Context.User.Identity.Name);

            if (string.IsNullOrEmpty(userId)) return;

            MessageContent message = new MessageContent
            {
                MessageId = GenerateMessageId(),
                UserId = userId,
                Content = content,
                SmallUser = messageContext.SmallUsers.Find(userId)
            };

            if (Messages.ContainsKey(conversationId))
            {
                Messages[conversationId].Add(message);
                return;
            }

            Messages.Add(conversationId, new List<MessageContent>{ message });
        }

        List<HubMessage> GetMessageHistory(string conversationId)
        {
            if (string.IsNullOrEmpty(conversationId)) return null;
            
            if(Messages.ContainsKey(conversationId))
            {
                return Messages[conversationId].Select(m => new HubMessage { Sender = GetUsername(m.UserId).Equals(Context.User.Identity.Name)? "": GetUsername(m.UserId), Content = m.Content, SentData = DateTime.Now }).ToList();
            }

            return null;
        }

        /*MessageContent GetMessageContent(*//*string messageId, *//*string conversationId,)
        {

        }*/

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

        string GenerateMessageId()
        {
            return GenerateId(300);
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
            // We are leaving it here for now
            public List<HubMessage> Messages { get; set; }
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
            public string Content { get; set; }
            public string Sender { get; set; }
            public DateTime SentData { get; set; }
        }
    }
}
