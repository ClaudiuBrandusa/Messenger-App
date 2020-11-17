using System;
using System.Collections.Generic;
using Messenger_Mobile_App.ViewModels;
using Messenger_Mobile_App.Views;
using Xamarin.Forms;

namespace Messenger_Mobile_App
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(ConversationsPage), typeof(ConversationsPage));
            Routing.RegisterRoute(nameof(ConversationPage), typeof(ConversationPage));
            Routing.RegisterRoute(nameof(NewContactPage), typeof(NewContactPage));
            Routing.RegisterRoute(nameof(NewConversationPage), typeof(NewConversationPage));
        }

        private async void OnMenuItemClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
