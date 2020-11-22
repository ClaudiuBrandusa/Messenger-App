using Messenger_Mobile_App.Models;
using Messenger_Mobile_App.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Messenger_Mobile_App.Views
{
    public partial class ConversationPage : ContentPage
    {
        ConversationViewModel _viewModel;
        public ConversationPage()
        {
            InitializeComponent();
            Shell.SetTabBarIsVisible(this, false); // This way we hide the tab bat when we are using this page

            BindingContext = _viewModel = new ConversationViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }
    }
}