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

        public RegisterViewModel()
        {
            LoginCommand = new Command(OnLoginCommand);
            RegisterCommand = new Command(OnRegisterCommand);
        }

        async void OnLoginCommand()
        {
            await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
        }

        async void OnRegisterCommand()
        {
            await Shell.Current.GoToAsync($"//{nameof(ConversationsPage)}");
        }
    }
}
