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

        static readonly int PacketMaxMessages = 10;

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
            int before = GetNextPacketNumber(conversationId);
            StoreMessage(conversationId, message);
            int after = GetNextPacketNumber(conversationId);

            int packetNumber = before > -1 ? before - 1 : -1;

            if(before != after) // then it means that we stored the message in a new packet
            {
                packetNumber++;
            }

            var data = new HubListConversation
            {
                Id = conversationId,
                ConversationName = GetConversationName(conversationId)
            };

            foreach(var connection in ownConnectionsId)
            {
                await Clients.Client(connection).SendAsync("SendMessage", conversationId, message, packetNumber);
                if (firstInteraction) await Clients.Client(connection).SendAsync("AddConversationInList", data);
            }

            List<string> receiverIds = GetConversationMembersIds(conversationId);

            foreach(var receiver in receiverIds)
            {
                if(CurrentUsers.ContainsKey(receiver) && !receiver.Equals(GetUserId(username)))
                {
                    await Clients.Group(receiver).SendAsync("ReceiveMessage", conversationId, message, packetNumber);
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

            var data = new HubChatroomConversation { Id = conversationId, ConversationName = GetConversationName(conversationId), Packets = GetMessageHistory(conversationId), Members = GetConversationMembersIds(conversationId) };

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

                if(conversation != null)
                {
                    foreach (var con in conversation)
                    {
                        if (IsGroupConversation(con.ConversationId)) continue;
                        conversation = new List<ConversationMember>() {con};
                        break;
                    }
                }

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

            Conversation member = GenerateEmptyConversation(userId, true);

            AddConversation(new List<Conversation>() { member });

            await Clients.Caller.SendAsync("EnterConversation", new HubChatroomConversation { Id = member.ConversationId, ConversationName = "New conversation"}, true);
        }

        // Helper methods

        List<HubListConversation> GetAllHubConversations(string userId, bool empty=false)
        {
            if(string.IsNullOrEmpty(userId))
            {
                return null;
            }

            var conversations = GetAllConversations(userId, empty);
            
            if (conversations == null) return null;

            var hubConversations = conversations
                .Select(c => new HubListConversation { ConversationName = GetConversationName(c.ConversationId), 
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
                conversations = conversations.Where(c => !IsConversationEmpty(c.ConversationId) || IsGroupConversation(c.ConversationId)).ToList();
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

        bool AddConversationDetailsToContext(ConversationDetail conversationDetail)
        {
            if (conversationDetail == null || conversationDetail == default) return false;
            if (!IsConversationIdValid(conversationDetail.ConversationId)) return false;
            if (messageContext.ConversationDetails.FirstOrDefault(d => d.ConversationId.Equals(conversationDetail.ConversationId)) != default) return false;

            try
            {
                messageContext.ConversationDetails.Add(conversationDetail);
                messageContext.SaveChanges();
            }
            catch
            {
                return false;
            }
            return true;
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
                return false;
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

            possibleConversations = possibleConversations.Where(c => !IsGroupConversation(c.ConversationId)).ToList();

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
            return messageContext.Packets.FirstOrDefault(c => c.ConversationId.Equals(conversationId)) == default; //!Messages.ContainsKey(conversationId) || Messages[conversationId].Count == 0;
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

            var conversationDetails = new ConversationDetail
            {
                ConversationId = conversationId,
                isGroup = isGroup,
                Conversation = conversation,
                ConversationName = isGroup ? "new conversation" : ""
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

            if(!AddConversationDetailsToContext(conversationDetails))
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

        bool AddMessageToPacket(string packetId, string conversationId, MessageContent message)
        {
            if (string.IsNullOrEmpty(packetId)) return false;
            if (string.IsNullOrEmpty(conversationId)) return false;
            if (message == null || message == default) return false;

            if (messageContext.PacketContents.Count(m => m.PacketId.Equals(packetId)) >= PacketMaxMessages) return false;

            Packet packet = GetPacket(packetId, conversationId);

            PacketContent packetContent = null;

            if (packet != null)
            {
                // then we found a packet
                packetContent = GetPacketContent(message, packetId, false);
            }
            else // otherwise we are creating another packet
            {
                packet = GeneratePacket(packetId, conversationId);
                try
                {
                    messageContext.Packets.Add(packet);
                    messageContext.SaveChanges();
                }catch
                {
                    return false;
                }
                if (packet == null) return false;
                packetContent = GetPacketContent(message, packetId, false);
            }

            if (packetContent == null) return false; // then something went wrong

            packetContent.Packet = packet;

            try
            {
                messageContext.PacketContents.Add(packetContent);
                messageContext.SaveChanges();
            }
            catch(Exception exception)
            {
                Console.WriteLine(exception.StackTrace);
                return false;
            }
            return true;
        }

        Packet GeneratePacket(string packetId, string conversationId)
        {
            if (string.IsNullOrEmpty(packetId)) return null;
            if (string.IsNullOrEmpty(conversationId)) return null;
            if (IsPacketIdUsed(packetId)) return null;
            int packetNumber = GetNextPacketNumber(conversationId);
            Packet packet = new Packet
            {
                PacketId = packetId,
                ConversationId = conversationId,
                Conversation = GetConversation(conversationId).First(),
                PacketNumber = packetNumber == -1 ? 0 : packetNumber
            };
            return packet;
        }

        Packet GetPacket(string conversationId, int packetNumber=-1)
        {
            if (string.IsNullOrEmpty(conversationId)) return null;
            if (packetNumber != -1 && (packetNumber < -1 || packetNumber > GetNextPacketNumber(conversationId) - 1)) return null;


            var packet = messageContext.Packets.FirstOrDefault(p => p.ConversationId.Equals(conversationId) && p.PacketNumber == (packetNumber == -1 ? GetNextPacketNumber(conversationId) - 1 : packetNumber));

            if (packet == default) return null;

            return packet;
        }

        Packet GetPacket(string packetId, string conversationId)
        {
            if (string.IsNullOrEmpty(packetId)) return null;
            if (string.IsNullOrEmpty(conversationId)) return null;
            var packet = messageContext.Packets.FirstOrDefault(p => p.PacketId.Equals(packetId) && p.ConversationId.Equals(conversationId));
            if (packet == default) return null;
            return packet;
        }

        int GetPacketCount(Packet packet)
        {
            if (packet == null || packet == default) return PacketMaxMessages;
            return messageContext.PacketContents.Count(m => m.PacketId.Equals(packet.PacketId));
        }

        int GetPacketCount(string packetId)
        {
            if (string.IsNullOrEmpty(packetId)) return -1;
            return messageContext.PacketContents.Count(m => m.PacketId.Equals(packetId));
        }

        bool IsPacketIdValid(string packetId)
        {
            if (string.IsNullOrEmpty(packetId)) return false;
            return true;
        }

        bool IsPacketIdUsed(string packetId)
        {
            if (IsPacketIdValid(packetId) && messageContext.Packets.FirstOrDefault(p => p.PacketId.Equals(packetId)) != default) return true;
            return false;
        }

        // Disabled
        void AddPacketToContext(string packetId, string conversationId)
        {
            /*if (string.IsNullOrEmpty(packetId)) return;
            if (string.IsNullOrEmpty(conversationId)) return;

            if (!Packets.ContainsKey(conversationId)) return;

            var packet = Packets[conversationId].FirstOrDefault(p => p.PacketId.Equals(packetId));

            if (packet == default) return;

            if (messageContext.Packets.Find(packetId) == null)
            {
                // then we 

            }*/
        }

        int GetNextPacketNumber(string conversationId)
        {
            if (string.IsNullOrEmpty(conversationId)) return -1;
            var seq = messageContext.Packets.Where(p => p.ConversationId.Equals(conversationId));
            if (!seq.Any()) return -1;
            return seq.Max(p => p.PacketNumber) + 1;
        }

        string GetCurrentPacketId(string conversationId)
        {
            if (string.IsNullOrEmpty(conversationId)) return "";
            var currentPacket = messageContext.Packets.FirstOrDefault(p => p.ConversationId.Equals(conversationId) && p.PacketNumber == GetNextPacketNumber(conversationId)-1);
            if (currentPacket == default) return "";
            if (GetPacketCount(currentPacket) >= PacketMaxMessages) return "";
            return currentPacket.PacketId;
        }

        // Packet Content

        PacketContent GetPacketContent(MessageContent message, string packetId, bool save=true)
        {
            if (string.IsNullOrEmpty(packetId)) return null;
            if (!IsPacketIdValid(packetId)) return null;
            if (message == null || message == default) return null;
            PacketContent content = new PacketContent
            {
                PacketId = packetId,
                MessageId = message.MessageId,
                MessageContent = message,
                Packet = null
            };

            message.PacketContent = content;
            if(save)
            {
                try
                {
                    messageContext.PacketContents.Add(content);
                    messageContext.SaveChanges();
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.StackTrace);
                    return null;
                }
            }

            return content;
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
                SmallUser = messageContext.SmallUsers.Find(userId),
                SentDate = DateTime.Now
            };

            string packetId = GetCurrentPacketId(conversationId);
            
            if(string.IsNullOrEmpty(packetId))
            {
                packetId = GeneratePacketId();
                while (IsPacketIdUsed(packetId)) packetId = GeneratePacketId();
            }

            if (!AddMessageToPacket(packetId, conversationId, message))
            {
                while (IsPacketIdUsed(packetId)) packetId = GeneratePacketId();
                if(!AddMessageToPacket(packetId, conversationId, message)) return; // no it should get added successfully, otherwise something went wrong
            }
        }

        List<HubMessagePacket> GetMessageHistory(string conversationId)
        {
            if (string.IsNullOrEmpty(conversationId)) return null;
            if (!IsConversationIdValid(conversationId)) return null;

            var lastPacket = GetPacket(conversationId, -1);

            if (lastPacket == null) return null;

            var packets = new List<Packet>() { lastPacket };

            if(lastPacket.PacketNumber != 0)
            {
                if(GetPacketCount(lastPacket) < 5)
                {
                    // then we load another one
                    packets.Add(GetPacket(conversationId, lastPacket.PacketNumber - 1));
                }
            }

            var new_packets = packets.Select(p => 
                new HubMessagePacket
                {
                    Messages = messageContext.PacketContents.Where(pc => pc.PacketId.Equals(p.PacketId)).Select(m =>
                    new HubMessage 
                    { 
                        Sender = messageContext.MessageContents.Where(mc => mc.MessageId.Equals(m.MessageId)).Select(mc => mc.UserId).FirstOrDefault(), 
                        SentData = messageContext.MessageContents.Where(mc => mc.MessageId.Equals(m.MessageId)).Select(mc => mc.SentDate).FirstOrDefault(), 
                        Content = messageContext.MessageContents.Where(mc => mc.MessageId.Equals(m.MessageId)).Select(mc => mc.Content).FirstOrDefault()
                    }).ToList().OrderBy(e => e.SentData).ToList(),
                    PacketId = p.PacketId,
                    PacketNumber = p.PacketNumber
                }).ToList();

            foreach(var p in new_packets)
            {
                foreach(var m in p.Messages)
                {
                    if (IsCurrentUsersId(m.Sender)) m.Sender = "";
                }
            }

            return new_packets;
        }

        /*MessageContent GetMessageContent(*//*string messageId, *//*string conversationId,)
        {

        }*/

        bool IsCurrentUsersId(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            return GetUsername(id).Equals(Context.User.Identity.Name);
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

        string GeneratePacketId()
        {
            return GenerateId(350);
        }

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
            public List<HubMessagePacket> Packets { get; set; }
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
