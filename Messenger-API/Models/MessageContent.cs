using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Messenger_API.Models
{
    public class MessageContent
    {
        public int MessageId { get; set; }
        public string Content { get; set; }
        public int UserId { get; set; }
        public DateTime SentDate { get; set; }

        public SmallUser SmallUser { get; set; }
        public PacketContent PacketContent { get; set; }
    }
}
