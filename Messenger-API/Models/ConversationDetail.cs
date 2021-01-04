using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Messenger_API.Models
{
    public class ConversationDetail
    {
        public string ConversationId { get; set; }
        public string ConversationName { get; set; }
        public bool isGroup { get; set; }

        public Conversation Conversation { get; set; }
    }
}
