using Messenger_API.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Messenger_API.Models
{
    public class Conversation
    {   
        public string ConversationId { get; set; }
        public ICollection<Packet> Packets { get; set; }
        public ConversationDetail ConversationDetail { get; set; }
        public BlockedContact BlockedContact { get; set; }
    }
}
