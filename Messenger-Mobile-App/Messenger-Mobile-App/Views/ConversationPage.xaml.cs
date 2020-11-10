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
            BindingContext = _viewModel = new ConversationViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }
    }
}