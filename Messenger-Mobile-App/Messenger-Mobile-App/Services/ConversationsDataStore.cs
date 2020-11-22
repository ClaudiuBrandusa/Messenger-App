using Messenger_Mobile_App.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger_Mobile_App.Services
{
    public class ConversationsDataStore : IDataStore<Conversation>
    {
        readonly List<Conversation> conversations;

        public ConversationsDataStore()
        {
            conversations = new List<Conversation>()
            {
                new Conversation
                {
                    Id = "a",
                    Contact = new Contact { Name = "Florin", IsActive = false, ImageUrl="https://d.newsweek.com/en/full/1585616/google-meet-logo.jpg?w=1600&h=1600&q=88&f=24975ba158fe3f926e521f5b86227d7a"},
                    Messages = "Interesant"
                },
                new Conversation
                {
                    Id = "b",
                    Contact = new Contact { Name = "Radu", IsActive = true, ImageUrl="https://upload.wikimedia.org/wikipedia/commons/thumb/a/a5/Google_Chrome_icon_%28September_2014%29.svg/1200px-Google_Chrome_icon_%28September_2014%29.svg.png" },
                    Messages = "I'm good"
                }
            };
        }

        public async Task<bool> AddItemAsync(Conversation item)
        {
            if(!conversations.Contains(item))
            {
                conversations.Add(item);
            }

            return await Task.FromResult(true);
        }

        public async Task<bool> DeleteItemAsync(string id)
        {
            var oldItem = conversations.Where((Conversation conversation) => conversation.Contact.Name.Equals(id)).FirstOrDefault();
            conversations.Remove(oldItem);

            return await Task.FromResult(true);
        }

        public async Task<Conversation> GetItemAsync(string id)
        {
            return await Task.FromResult(conversations.FirstOrDefault(c => c.Contact.Name.Equals(id)));
        }

        public async Task<IEnumerable<Conversation>> GetItemsAsync(bool forceRefresh = false)
        {
            return await Task.FromResult(conversations);
        }

        public async Task<bool> UpdateItemAsync(Conversation item)
        {
            var oldItem = conversations.Where((Conversation conversation) => conversation.Contact.Name.Equals(item.Contact.Name)).FirstOrDefault();
            conversations.Remove(oldItem);
            conversations.Add(item);

            return await Task.FromResult(true);
        }
    }
}
