using Messenger_Mobile_App.Models;
using Messenger_Mobile_App.Services;
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
            ConversationTappedCommand = new Command<Conversation>(OnConversationTapped);

            DependencyService.Get<ChatService>().Connect();
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

        public void OnAppearing()
        {
            IsBusy = true;
            ExecuteLoadConversationsCommand();
        }
        
        async void OnConversationTapped(Conversation conversation)
        {
            if (conversation == null)
            {
                return;
            }

            if (conversation == null || conversation == default)
            {
                return;
            }

            await Shell.Current.GoToAsync($"{nameof(ConversationPage)}?{nameof(ConversationViewModel.Name)}={conversation.Contact.Name}");
        }
    }
}
