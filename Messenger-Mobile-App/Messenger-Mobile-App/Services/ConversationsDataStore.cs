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
                    Contact = new Contact { Name = "Radu", IsActive = true, ImageUrl="profile1.png" },
                    Messages = "I'm good",
                    LastMessageDate = new DateTime(2020, 11, 22)
                },
                new Conversation
                {
                    Id = "b",
                    Contact = new Contact { Name = "Florin", IsActive = false},
                    Messages = "Interesant",
                    LastMessageDate = new DateTime(2020, 10, 14)
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
