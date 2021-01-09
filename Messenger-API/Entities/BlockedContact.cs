using Messenger_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Messenger_API.Entities
{
    public class BlockedContact
    {
        public string UserId { get; set; }
        public string ConversationId { get; set; }
        public Conversation Conversation { get; set; }
        public SmallUser User { get; set; }
    }
}
