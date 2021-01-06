using Messenger_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Messenger_API.Entities
{
    public class ConversationMember
    {
        public string ConversationId { get; set; }
        public string UserId { get; set; }
        public bool IsAdmin { get; set; }

        public SmallUser SmallUser { get; set; }
    }
}
