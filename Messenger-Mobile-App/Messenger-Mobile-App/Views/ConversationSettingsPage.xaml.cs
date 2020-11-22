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
	[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConversationSettingsPage : ContentPage
    {
        ConversationSettingsViewModel _viewModel;
        public ConversationSettingsPage()
        {
            InitializeComponent();
            Shell.SetTabBarIsVisible(this, false);
            BindingContext = _viewModel = new ConversationSettingsViewModel(); 
        }
    }
}