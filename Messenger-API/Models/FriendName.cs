using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Messenger_API.Models
{
    public class FriendName
    {
        public int FriendId { get; set; }
        public string UserId { get; set; }

        public SmallUser SmallUser { get; set; }
        public Friend Friend { get; set; }
    }
}
