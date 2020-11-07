﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Messenger_API.Models
{
    public class Packet
    {   
        public int PacketId { get; set; }
        public int ConversationId { get; set; }
        public int PacketNumber { get; set; }
        
        public ICollection<PacketContent> PacketContents { get; set; }
        public Conversation Conversation { get; set; }
    }
}
