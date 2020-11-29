using Messenger_Mobile_App.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Messenger_Mobile_App.Services
{
    public class CurrentUser // just a model for now
    {
        User User { get; set; }

        public CurrentUser()
        {
            User = new User { Name = "Claudiu", ImgUrl = "default_user_image.png" };
        }
    }
}
