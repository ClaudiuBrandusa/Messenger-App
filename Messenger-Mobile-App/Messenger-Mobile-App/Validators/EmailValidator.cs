using System;
using System.Collections.Generic;
using System.Text;

namespace Messenger_Mobile_App.Validators
{
    public class EmailValidator
    {
        public static bool Check(string email)
        {
            return !String.IsNullOrEmpty(email);
        }
    }
}
