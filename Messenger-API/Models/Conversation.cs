using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Messenger_API.Models
{
    public class Conversation
    {   
        public int ConversationId { get; set; }
        public int UserId { get; set; }

        public SmallUser SmallUser { get; set; }
        public ICollection<ConversationAdmin> ConversationAdmins { get; set; }
        public ICollection<Packet> Packets { get; set; }
    }
}
