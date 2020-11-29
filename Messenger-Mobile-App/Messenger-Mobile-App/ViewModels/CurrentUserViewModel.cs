using Messenger_Mobile_App.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Messenger_Mobile_App.ViewModels
{
    [QueryProperty(nameof(User), nameof(User))]
    public class CurrentUserViewModel : BaseViewModel
    {
        public User User;


    }
}
