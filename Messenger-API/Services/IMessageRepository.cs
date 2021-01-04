using Messenger_API.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Messenger_API.Services
{
    public interface IMessageRepository
    {
        IEnumerable<Friend> GetAllFriends(string userId);
        string GetIdByName(string userName);
        bool AcceptFriendRequest(string userId, string friendId);
        bool SendFriendRequest(string userId, string friendId);
        bool RemoveFriend(string userId, string friendId);
        bool AddImageProfile(ImageProfile imageProfile, List<IFormFile> Image);
    }
}
