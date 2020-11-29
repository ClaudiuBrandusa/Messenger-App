using System;
using System.Collections.Generic;
using System.Text;

namespace Messenger_Mobile_App.Models
{
    public class User
    {
        public string Name { get; set; }
        public string ImgUrl { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
