using Messenger_Mobile_App.Models;
using Messenger_Mobile_App.Services;
using Messenger_Mobile_App.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Messenger_Mobile_App.ViewModels
{
    public class ProfilePageViewModel : BaseViewModel
    {
        public Command GoBackCommand { get; }

        public User User { get; }

        public ProfilePageViewModel()
        {
            Title = "Profile";
            User = DependencyService.Get<CurrentUser>().GetUser();
            GoBackCommand = new Command(async () => await BackCommand());
        }

        async Task BackCommand()
        {
            await Shell.Current.GoToAsync($"//{nameof(ConversationsPage)}");
        }

    }
}
