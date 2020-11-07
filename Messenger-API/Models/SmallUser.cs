using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Messenger_API.Models
{
    public class SmallUser
    {
        public int UserId { get; set; }
        public string UserName { get; set; }

        public ICollection<FriendName> FriendNames { get; set; }
        public ICollection<MessageContent> MessageContents { get; set; }
        public ICollection<Conversation> Conversations { get; set; }
        public ICollection<ConversationAdmin> ConversationAdmins { get; set; }
    }
}
