using System;
using System.Collections.Generic;
using System.Text;

namespace Messenger_Mobile_App.Models
{
    public class Contact
    {
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
