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
	[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConversationsPage : ContentPage
    {
        public List<Conversation> Conversations { get; private set; }
     
        ConversationsViewModel _viewModel;
        
        public ConversationsPage()
        {
            InitializeComponent();
            //BindingContext = this;//_viewModel = new ConversationsViewModel();

            Conversations = new List<Conversation>();
            Conversations.Add(new Conversation 
            {
                Id = "a",
                Contact = new Contact { Name = "Gicu", IsActive = false, ImageUrl="https://d.newsweek.com/en/full/1585616/google-meet-logo.jpg?w=1600&h=1600&q=88&f=24975ba158fe3f926e521f5b86227d7a"},
                Messages = "Sal kf?"
            });

            Conversations.Add(new Conversation
            {
                Id = "b",
                Contact = new Contact { Name = "Radu", IsActive = false, ImageUrl="https://upload.wikimedia.org/wikipedia/commons/thumb/a/a5/Google_Chrome_icon_%28September_2014%29.svg/1200px-Google_Chrome_icon_%28September_2014%29.svg.png" },
                Messages = "Sunt interese la mijloc.."
            });

            BindingContext = this;
        }

        // Keep in mind that the sender is the ListView

        void OnListViewItemTapped(object sender, ItemTappedEventArgs e)
        {
            Conversation tappedItem = e.Item as Conversation;
        }
    }
}