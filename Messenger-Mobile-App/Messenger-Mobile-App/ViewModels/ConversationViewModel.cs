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

        string url;

        public string Url
        {
            get => url;
            set
            {
                url = value;
                OnPropertyChanged();
            }
        }

        public bool active;

        public bool Active
        {
            get => active;
            set
            {
                active = value;
                OnPropertyChanged();
            }
        }


        public ObservableCollection<Message> Messages { get; private set; }

        public Command ReloadMessagesCommand { get; }

        public Command OpenConversationSettingsCommand { get; }

        public ConversationViewModel()
        {
            ReloadMessagesCommand = new Command(async () => await ReloadMessages());
            Messages = new ObservableCollection<Message>();
            OpenConversationSettingsCommand = new Command(async () => await EnterConversationSettings());
        }

        public async Task ReloadMessages()
        {
            IsBusy = true;
            try
            {
                Messages.Clear();
                // we are populating the list with hard coded messages
                Messages.Add(new Message { Content = "Hi", Sender = "You", Date = new DateTime(2020, 11, 22, 10, 0, 0)});
                Messages.Add(new Message { Content="Hi", Sender=Name, Date = new DateTime(2020, 11, 22, 10, 5, 0) });
                Messages.Add(new Message { Content = "What's up?", Sender = "You", Date = new DateTime(2020, 11, 22, 10, 12, 0) });
                Messages.Add(new Message { Content = "Nothing, you?", Sender = Name, Date = new DateTime(2020, 11, 22, 10, 53, 0) });
                Messages.Add(new Message { Content = "I'm good", Sender = "You", Date = new DateTime(2020, 11, 22, 11, 13, 0) });
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
                Title = name;
                Url = conversation.Contact.ImageUrl;
                contactImage = ImageSource.FromFile(Url);
                Active = conversation.Contact.IsActive;
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
            }
            IsBusy = false;
        }

        async Task EnterConversationSettings()
        {
            await Shell.Current.GoToAsync($"{nameof(ConversationSettingsPage)}?{nameof(ConversationSettingsViewModel.Name)}={conversation.Contact.Name}");
        }

        public void OnAppearing()
        {
            LoadConversation();
            ReloadMessages();
        }
    }
}
