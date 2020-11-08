using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Messenger_Mobile_App.Services;
using Messenger_Mobile_App.Views;

namespace Messenger_Mobile_App
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            DependencyService.Register<ContactsDataStore>();
            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
