using Messenger_Mobile_App.Services;
using Messenger_Mobile_App.Views;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Messenger_Mobile_App.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        public Command LoginCommand { get; }
        public Command RegisterCommand { get; }

        string username;
        public string Username 
        {
            get => username;
            set
            {
                username = value;
                OnPropertyChanged();
            }
        }
        string password;
        public string Password 
        {
            get => password;
            set
            {
                password = value;
                OnPropertyChanged();
            }
        }

        public LoginViewModel()
        {
            LoginCommand = new Command(OnLoginClicked);
            RegisterCommand = new Command(OnRegisterCommand);
        }

        bool ValidateInput()
        {
            if(!CurrentUser.ValidateUserName(Username))
            {
                Shell.Current.DisplayAlert("Warning", "Username is wrong or does not exist", "Back");
                return false;
            }
            if (!CurrentUser.ValidatePassword(Password))
            {
                Shell.Current.DisplayAlert("Warning", "Wrong password", "Back");
                return false;
            }

            return true;
        }

        private async void OnLoginClicked(object obj)
        {
            if (!ValidateInput())
            {
                return;
            }

            CurrentUser user = DependencyService.Get<CurrentUser>();

            user.SetUserName(Username);
            user.SetPassword(Password);

            // If something went wrong
            if(! await user.LoginAsync())
            {
                // then cancel the login process
                await Shell.Current.DisplayAlert("Unable to login", "There is no account with these credentials","Back");
                return;
            }

            // Then we clear the input data
            Username = "";
            Password = "";

            // Prefixing with `//` switches to a different navigation stack instead of pushing to the active one
            await Shell.Current.GoToAsync($"//{nameof(ConversationsPage)}");
        }

        async void OnRegisterCommand()
        {
            await Shell.Current.GoToAsync($"//{nameof(RegisterPage)}");
        }
    }
}
