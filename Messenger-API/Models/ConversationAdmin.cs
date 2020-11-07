using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Messenger_API.Models
{
    public class ConversationAdmin
    {
        public int ConversationId { get; set; }
        public int UserId{ get; set; }

        public Conversation Conversation { get; set; }
        public SmallUser SmallUser { get; set; }
    }
}
