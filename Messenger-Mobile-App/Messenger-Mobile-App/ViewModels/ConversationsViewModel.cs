using Messenger_Mobile_App.Models;
using Messenger_Mobile_App.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Messenger_Mobile_App.ViewModels
{
    public class ConversationsViewModel : BaseViewModel
    {
        public ObservableCollection<Conversation> Conversations { get; }
        public Command LoadConversationsCommand { get; }

        public Command AddContactCommand { get; }
        public Command<Conversation> ConversationTappedCommand { get; }
        
        public ConversationsViewModel()
        {
            Title = "Conversations";

            Conversations = new ObservableCollection<Conversation>();
            LoadConversationsCommand = new Command(async () => await ExecuteLoadConversationsCommand());
            AddContactCommand = new Command(OnAddConversation);
            ConversationTappedCommand = new Command<Conversation>(OnConversationClicked);
        }

        async Task ExecuteLoadConversationsCommand()
        {
            IsBusy = true;

            try
            {
                Conversations.Clear();
                var conversations = await DataConversations.GetItemsAsync(true);
                foreach (var conversation in conversations)
                {
                    Conversations.Add(conversation);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        async void OnAddConversation(object obj)
        {
            await Shell.Current.GoToAsync(nameof(NewConversationPage));
        }

        async void OnConversationClicked(Conversation conversation)
        {
            await Task.CompletedTask;
        }

        public void OnAppearing()
        {
            IsBusy = true;
            ExecuteLoadConversationsCommand();
        }
    }
}
