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
                OnPropertyChanged();
            }
        }
        ImageSource contactImage;
        public ImageSource ContactImage
        {
            get => contactImage;
            set
            {
                contactImage = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Message> Messages { get; private set; }

        public Command ReloadMessagesCommand { get; }

        public ConversationViewModel()
        {
            ReloadMessagesCommand = new Command(async () => await ReloadMessages());
            Messages = new ObservableCollection<Message>();
        }

        public async Task ReloadMessages()
        {
            IsBusy = true;
            try 
            {
                // we are populating the list with hard coded messages
                Messages.Add(new Message { Content="Hi", Sender="You"});
                Messages.Add(new Message { Content="Hi", Sender=Name});
                Messages.Add(new Message { Content = "What's up?", Sender = "You"});
                Messages.Add(new Message { Content = "Nothing, you?", Sender = Name});
                Messages.Add(new Message { Content = "I'm good", Sender = "You"});
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            IsBusy = false;
        }

        public async Task LoadConversation()
        {
            IsBusy = true;
            try
            {
                conversation = await DataConversations.GetItemAsync(Name);
                name = conversation.Contact.Name;
                contactImage = ImageSource.FromFile(conversation.Contact.ImageUrl);
            }catch(Exception ex)
            {
                Debug.WriteLine(ex);
            }
            IsBusy = false;
        }

        public void OnAppearing()
        {
            LoadConversation();
            ReloadMessages();
        }
    }
}
