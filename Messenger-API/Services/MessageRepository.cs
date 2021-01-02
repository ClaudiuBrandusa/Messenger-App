using Messenger_API.Data;
using Messenger_API.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Messenger_API.Services
{
    public class MessageRepository : IMessageRepository
    {
        private readonly MessageContext _context;

        public MessageRepository(MessageContext context)
        {
            _context = context;
        }

        public bool AcceptFriendRequest(string userId, string friendId)
        {
            if(userId == null)
            {
                return false;
            }
            if(friendId == null)
            {
                return false;
            }

            Friend friend = _context.Friends.FirstOrDefault(r => r.FriendId.Equals(friendId));

            if(friend == default)
            {
                return false;
            }

            friend.ConfirmedDate = DateTime.Now;

            _context.FriendNames.Add(new FriendName { UserId = userId, FriendId = friendId });
            
            _context.SaveChanges();

            return true;
        }

        public bool SendFriendRequest(string userId, string friendId)
        {
            if (userId == null)
            {
                return false;
            }
            if (friendId == null)
            {
                return false;
            }


            Friend friend = new Friend
            {
                FriendId = friendId,
                SentDate = DateTime.Now
            };

            _context.Friends.Add(friend);
            _context.SaveChanges();

            return true;
        }

        public IEnumerable<Friend> GetAllFriends(string userId)
        {
            throw new NotImplementedException();
        }

        public bool RemoveFriend(string userId, string friendId)
        {
            if(userId == null)
            {
                return false;
            }
            if(friendId == null)
            {
                return false;
            }

            FriendName friendName = _context.FriendNames.FirstOrDefault(r => r.FriendId == friendId);
            _context.FriendNames.Remove(friendName);

            Friend friend = _context.Friends.FirstOrDefault(r => r.FriendId == friendId);
            _context.Friends.Remove(friend);

            _context.SaveChanges();

            return true;
        }

        public string GetIdByName(string userName)
        {
            if(userName == null)
            {
                return "";
            }

            var user = _context.SmallUsers.FirstOrDefault(r => r.UserName.Equals(userName));

            return user.UserId;
        }

        public bool AddImageProfile(ImageProfile imageProfile, List<IFormFile> Image)
        {
            if(imageProfile == null)
            {
                return false;
            }

            foreach (var item in Image)
            {
                if (item.Length > 0)
                {
                    using (var stream = new MemoryStream())
                    {
                        item.CopyToAsync(stream);
                        imageProfile.Image = stream.ToArray();
                    }
                }
            }
            _context.ImageProfiles.Add(imageProfile);
            _context.SaveChanges();

            return true;
        }
    }
}
