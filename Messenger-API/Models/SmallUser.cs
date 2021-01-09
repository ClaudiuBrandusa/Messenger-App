using Messenger_API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Messenger_API.Models
{
    public class SmallUser
    {
        public string UserId { get; set; }
        public string UserName { get; set; }

        public ICollection<FriendName> FriendNames { get; set; }
        public ICollection<MessageContent> MessageContents { get; set; }
        public ICollection<ConversationMember> Conversations { get; set; }
        public ICollection<BlockedContact> BlockedContacts { get; set; }
        public ImageProfile ImageProfile { get; set; }
    }
}
