using Messenger_Mobile_App.Models;
using Messenger_Mobile_App.ViewModels;
using System;
using System.ComponentModel;
using System.Diagnostics;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Messenger_Mobile_App.Views
{
    public partial class ContactsPage : ContentPage
    {
        ContactsViewModel _viewModel;
        public ContactsPage()
        {
            InitializeComponent();

            BindingContext = _viewModel = new ContactsViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }
        public void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            Contact contact = e.Item as Contact;
            _viewModel.ContactTappedCommand.Execute(contact);
        }
    }

}