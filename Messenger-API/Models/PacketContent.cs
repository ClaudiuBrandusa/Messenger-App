using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Messenger_API.Models
{
    public class PacketContent
    { 
        public string PacketId { get; set; }
        public string MessageId { get; set; }

        public MessageContent MessageContent { get; set; }
        public Packet Packet { get; set; }
    }
}
