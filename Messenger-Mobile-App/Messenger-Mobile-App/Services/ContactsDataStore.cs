using Messenger_Mobile_App.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger_Mobile_App.Services
{
    public class ContactsDataStore : IDataStore<Contact>
    {
        readonly List<Contact> contacts;

        public ContactsDataStore()
        {
            contacts = new List<Contact>() // there goes the contacts loading
            {
                new Contact { Name = "Test", IsActive = true },
                new Contact { Name = "Radu", IsActive = false, ImageUrl="https://upload.wikimedia.org/wikipedia/commons/thumb/a/a5/Google_Chrome_icon_%28September_2014%29.svg/1200px-Google_Chrome_icon_%28September_2014%29.svg.png" },
                new Contact { Name = "Gicu", IsActive = false, ImageUrl="https://d.newsweek.com/en/full/1585616/google-meet-logo.jpg?w=1600&h=1600&q=88&f=24975ba158fe3f926e521f5b86227d7a"}
            };
        }

        public async Task<bool> AddItemAsync(Contact item)
        {
            contacts.Add(item);

            return await Task.FromResult(true);
        }

        public async Task<bool> DeleteItemAsync(string id)
        {
            var oldItem = contacts.Where((Contact contact) => contact.Name.Equals(id)).FirstOrDefault();
            contacts.Remove(oldItem);

            return await Task.FromResult(true);
        }

        public async Task<Contact> GetItemAsync(string id)
        {
            return await Task.FromResult(contacts.FirstOrDefault(c => c.Name.Equals(id)));
        }

        public async Task<IEnumerable<Contact>> GetItemsAsync(bool forceRefresh = false)
        {
            return await Task.FromResult(contacts);
        }

        public async Task<bool> UpdateItemAsync(Contact item)
        {
            var oldItem = contacts.Where((Contact contact) => contact.Name.Equals(item.Name)).FirstOrDefault();
            contacts.Remove(oldItem);
            contacts.Add(item);

            return await Task.FromResult(true);
        }
    }
}
