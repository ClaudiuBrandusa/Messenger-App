using Messenger_Mobile_App.Models;
using Messenger_Mobile_App.Services;
using Messenger_Mobile_App.ViewModels;
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
    public class ContactsViewModel : BaseViewModel
    {
        public ObservableCollection<Contact> Contacts { get; }

        public Command LoadContactsCommand { get; }
        
        public Command AddContactCommand { get; }

        public Command<Contact> ContactTappedCommand { get; }

        public ContactsViewModel()
        {
            Title = "Contacts";

            Contacts = new ObservableCollection<Contact>();
            LoadContactsCommand = new Command(async () => await ExecuteLoadContacts());
            AddContactCommand = new Command(OnAddContact);
            ContactTappedCommand = new Command<Contact>(OnContactSelected);
        }

        async Task ExecuteLoadContacts()
        {
            IsBusy = true;

            try
            {
                Contacts.Clear();
                var contacts = await DataContacts.GetItemsAsync(true);
                foreach(var contact in contacts)
                {
                    Contacts.Add(contact);
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

        public void OnAppearing()
        {
            IsBusy = true;
            ExecuteLoadContacts();
        }

        async void OnAddContact(object obj)
        {
            await Shell.Current.GoToAsync(nameof(NewContactPage));
        }

        async void OnContactSelected(Contact contact)
        {
            if(contact == null)
            {
                return;
            }

            var conversation = await DataConversations.GetItemAsync(contact.Name);

            if(conversation == null || conversation == default)
            {
                return;
            }

            await Shell.Current.GoToAsync($"{nameof(ConversationPage)}?{nameof(ConversationViewModel.Name)}={conversation.Contact.Name}");
        }
    }
}
