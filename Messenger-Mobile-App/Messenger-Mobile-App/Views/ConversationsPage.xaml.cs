using Messenger_Mobile_App.Models;
using Messenger_Mobile_App.Services;
using Messenger_Mobile_App.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Messenger_Mobile_App.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConversationsPage : ContentPage
    {
        public List<Conversation> Conversations { get; private set; }
     
        ConversationsViewModel _viewModel;
        
        public ConversationsPage()
        {
            InitializeComponent();

            Conversations = new List<Conversation>();

            BindingContext = _viewModel = new ConversationsViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }

        public void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            Conversation conversation = e.Item as Conversation;
            _viewModel.ConversationTappedCommand.Execute(conversation);
        }
    }
}