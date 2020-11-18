using Messenger_Mobile_App.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Messenger_Mobile_App.ViewModels
{
    [QueryProperty(nameof(Name), nameof(Name))] // We are using Name as index key for now
    public class ConversationViewModel : BaseViewModel
    {
        Conversation conversation;

        public string Test
        {
            get => conversation != null ? conversation.Id : "no";
        }

        string name;
        public string Name
        {
            get => name;
            set
            {
                name = value;
            }
        }
        public Command LoadConversationCommand;
        public async Task LoadConversation()
        {
            try
            {
                conversation = await DataConversations.GetItemAsync(Name);
            }catch(Exception ex)
            {
                Debug.WriteLine(ex);
            }
            IsBusy = false;
        }

        public void OnAppearing()
        {
            LoadConversation();
        }

        public ConversationViewModel()
        {
            IsBusy = true;

            LoadConversationCommand = new Command(async () => await LoadConversation());
        }
    }
}
