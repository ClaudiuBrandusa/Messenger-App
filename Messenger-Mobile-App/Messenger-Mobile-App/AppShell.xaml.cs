using System;
using System.Collections.Generic;
using Messenger_Mobile_App.Models;
using Messenger_Mobile_App.ViewModels;
using Messenger_Mobile_App.Views;
using Xamarin.Forms;

namespace Messenger_Mobile_App
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public User User { get; set; }
        public AppShell()
        {
            InitializeComponent();

            User = DependencyService.Get<User>();

            Routing.RegisterRoute(nameof(ConversationPage), typeof(ConversationPage));
            Routing.RegisterRoute(nameof(ConversationSettingsPage), typeof(ConversationSettingsPage));
            Routing.RegisterRoute(nameof(NewContactPage), typeof(NewContactPage));
            Routing.RegisterRoute(nameof(NewConversationPage), typeof(NewConversationPage));
        }

        private async void OnMenuLogoutClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//LoginPage");
        }
        private async void OnMenuSettingsClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//SettingsPage");
        }
        private async void OnMenuProfileClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//ProfilePage");
        }
    }
}
