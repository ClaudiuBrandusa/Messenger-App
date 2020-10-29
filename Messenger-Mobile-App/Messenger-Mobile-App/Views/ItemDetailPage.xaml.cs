using System.ComponentModel;
using Xamarin.Forms;
using Messenger_Mobile_App.ViewModels;

namespace Messenger_Mobile_App.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}