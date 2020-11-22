using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Messenger_Mobile_App.ViewModels
{
    [QueryProperty(nameof(Name), nameof(Name))] // We are using Name as index key for now
    public class ConversationSettingsViewModel : BaseViewModel
    {
        string name;
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        public ConversationSettingsViewModel()
        {
            Title = "Conversation Settings";
        }
    }
}
