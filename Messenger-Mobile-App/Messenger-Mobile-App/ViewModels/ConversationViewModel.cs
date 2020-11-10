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

        string name;
        public string Name
        {
            get => name;
            set
            {
                name = value;
                LoadConversation();
            }
        }

        public async void LoadConversation()
        {
            try
            {
                conversation = await DataConversations.GetItemAsync(Name);
            }catch(Exception ex)
            {
                Debug.WriteLine(ex);
            }
            IsBusy = false;
            Console.WriteLine("here 2");
        }

        public void OnAppearing()
        {
            Console.WriteLine("here 1");
            IsBusy = true;
            LoadConversation();
        }
    }
}
