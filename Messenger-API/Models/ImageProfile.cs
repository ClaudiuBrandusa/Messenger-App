using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Messenger_API.Models
{
    public class ImageProfile
    {
        public string UserId { get; set; }
        public byte[] Image { get; set; }

        public SmallUser SmallUser { get; set; }
    }
}
