using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Messenger_API.Models
{
    public class Friend
    {
        public string FriendId { get; set; }
        public DateTime SentDate { get; set; }
        public DateTime ConfirmedDate { get; set; }

        public ICollection<FriendName> FriendNames { get; set; }
    }
}
