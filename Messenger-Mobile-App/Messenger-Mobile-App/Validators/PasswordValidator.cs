using System;
using System.Collections.Generic;
using System.Text;

namespace Messenger_Mobile_App.Validators
{
    public class PasswordValidator
    {
        public static bool Check(string password)
        {
            return !String.IsNullOrEmpty(password);
        }
    }
}
