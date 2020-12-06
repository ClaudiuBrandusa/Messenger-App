using System;
using System.Collections.Generic;
using System.Text;

namespace Messenger_Mobile_App.Validators
{
    public class UsernameValidator
    {
        public static bool Check(string username)
        {
            return !String.IsNullOrEmpty(username);
        }
    }
}
