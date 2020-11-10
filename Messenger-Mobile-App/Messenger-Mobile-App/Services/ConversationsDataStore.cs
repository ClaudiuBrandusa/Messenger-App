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
                new Conversation {Id="Test", Contact = new Contact{ Name = "Test" }, Messages = "Hi!"}
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
            var oldItem = conversations.Where((Conversation conversation) => conversation.Id.Equals(id)).FirstOrDefault();
            conversations.Remove(oldItem);

            return await Task.FromResult(true);
        }

        public async Task<Conversation> GetItemAsync(string id)
        {
            return await Task.FromResult(conversations.FirstOrDefault(c => c.Id.Equals(id)));
        }

        public async Task<IEnumerable<Conversation>> GetItemsAsync(bool forceRefresh = false)
        {
            return await Task.FromResult(conversations);
        }

        public async Task<bool> UpdateItemAsync(Conversation item)
        {
            var oldItem = conversations.Where((Conversation conversation) => conversation.Id.Equals(item.Id)).FirstOrDefault();
            conversations.Remove(oldItem);
            conversations.Add(item);

            return await Task.FromResult(true);
        }
    }
}
