using Messenger_Mobile_App.Views;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Messenger_Mobile_App.ViewModels
{
    public class SettingsPageViewModel : BaseViewModel
    {
        public Command GoBackCommand { get; }

        public SettingsPageViewModel()
        {
            Title = "Settings";
            GoBackCommand = new Command(async () => await BackCommand());
        }

        async Task BackCommand()
        {
            await Shell.Current.GoToAsync($"//{nameof(ConversationsPage)}");
        }
    }
}
