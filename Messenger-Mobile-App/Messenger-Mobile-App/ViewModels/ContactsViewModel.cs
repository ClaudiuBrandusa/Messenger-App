using Messenger_Mobile_App.Models;
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
        Contact _selectedContact;
        public Contact SelectedContact { get { return _selectedContact; } }

        public ObservableCollection<Contact> Contacts { get; }

        public Command LoadContactsCommand { get; }
        
        public Command AddContactCommand { get; }

        public ContactsViewModel()
        {
            Title = "Contacts";

            Contacts = new ObservableCollection<Contact>();
            LoadContactsCommand = new Command(async () => await ExecuteLoadContactsCommand());
            AddContactCommand = new Command(OnAddContact);
        }

        Task ExecuteLoadContactsCommand()
        {
            IsBusy = true;

            try
            {
                Contacts.Clear();
                Contacts.Add(new Contact { Name = "Test", IsActive = true });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }

            return Task.CompletedTask;
        }

        public void OnAppearing()
        {
            IsBusy = true;
            _selectedContact = null;
        }

        async void OnAddContact(object obj)
        {
            await Shell.Current.GoToAsync(nameof(NewContactPage)); // I do get wrong uri after accessing this route
        }
    }
}
