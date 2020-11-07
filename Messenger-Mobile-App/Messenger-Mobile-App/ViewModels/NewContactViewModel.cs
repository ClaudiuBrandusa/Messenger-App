using Messenger_Mobile_App.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Messenger_Mobile_App.ViewModels
{
    public class NewContactViewModel : BaseViewModel
    {
        string name;
        bool status;
        public Command SaveCommand { get; }
        public Command CancelCommand { get; }

        public NewContactViewModel()
        {
            SaveCommand = new Command(OnSave, ValidateSave);
            CancelCommand = new Command(OnCancel); 
            this.PropertyChanged +=
                 (_, __) => SaveCommand.ChangeCanExecute();
        }

        bool ValidateSave()
        {
            return !String.IsNullOrWhiteSpace(name);
        }

        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        public bool Status
        {
            get => status;
            set => SetProperty(ref status, value);
        }

        async void OnCancel()
        {
            await Shell.Current.GoToAsync(".."); // Go back
        }

        async void OnSave()
        {
            Contact newContact = new Contact()
            {
                Name = Name,
                IsActive = Status
            };

            await DataContacts.AddItemAsync(newContact);

            await Shell.Current.GoToAsync(".."); // Go back
        }
    }
}
