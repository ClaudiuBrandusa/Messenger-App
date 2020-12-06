using Messenger_Mobile_App.Services;
using Messenger_Mobile_App.Views;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Messenger_Mobile_App.ViewModels
{
    public class RegisterViewModel : BaseViewModel
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

        string confirmPassword;
        public string ConfirmPassword
        {
            get => confirmPassword;
            set
            {
                confirmPassword = value;
                OnPropertyChanged();
            }
        }

        string email;
        public string Email
        {
            get => email;
            set
            {
                email = value;
                OnPropertyChanged();
            }
        }

        public RegisterViewModel()
        {
            LoginCommand = new Command(OnLoginCommand);
            RegisterCommand = new Command(OnRegisterCommand);
        }

        bool ValidateInput()
        {
            if (!CurrentUser.ValidateUserName(Username))
            {
                Shell.Current.DisplayAlert("Warning", "Username is wrong or does not exist", "Back");
                return false;
            }
            if (!CurrentUser.ValidatePassword(Password))
            {
                Shell.Current.DisplayAlert("Warning", "Wrong password", "Back");
                return false;
            }
            if (Password != ConfirmPassword)
            {
                Shell.Current.DisplayAlert("Warning", "Confirm password must match with password", "Back");
                return false;
            }
            if (!CurrentUser.ValidateEmail(Email))
            {
                Shell.Current.DisplayAlert("Warning", "Wrong email", "Back");
                return false;
            }

            return true;
        }

        async void OnLoginCommand()
        {
            await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
        }

        async void OnRegisterCommand()
        {
            if (!ValidateInput())
            {
                return;
            }

            CurrentUser user = DependencyService.Get<CurrentUser>();

            user.SetUserName(Username);
            user.SetPassword(Password);
            user.SetEmail(Email);

            // If something went wrong
            if (!user.Register())
            {
                // then cancel the login process
                await Shell.Current.DisplayAlert("Unable to register", "You cannot create an account with these credentials", "Back");
                return;
            }

            // Then we clear the input data
            Username = "";
            Password = "";
            ConfirmPassword = "";
            Email = "";

            await Shell.Current.GoToAsync($"//{nameof(ConversationsPage)}");
        }
    }
}
