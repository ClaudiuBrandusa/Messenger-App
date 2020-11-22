using System;
using System.Collections.Generic;
using System.Text;

namespace Messenger_Mobile_App.Models
{
    public class Conversation
    {
        public string Id { get; set; }
        public Contact Contact { get; set; }
        public string Messages { get; set; }
        public DateTime LastMessageDate { get; set; }

        public override string ToString()
        {
            return Contact.ToString();
        }
    }
}
