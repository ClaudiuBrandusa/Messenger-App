using Messenger_API.Data;
using Messenger_API.Entities;
using Messenger_API.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Messenger_API.Services
{
    public class UserRepository : IUserRepository, ITokenRepository
    {
        private readonly UserContext _context;
        private readonly IConfiguration _configuration;

        public UserRepository(UserContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public bool ExistRefreshToken(string refreshToken)
        {
            return _context.RefreshTokens.Find(refreshToken) != null;
        }

        public bool IsRefreshTokenOwnedByUserId(string userId, string refreshToken)
        {
            Console.WriteLine(refreshToken);
            var x = _context.RefreshTokens.FirstOrDefault(t => t.Token.Equals(refreshToken));

            if (x == default)
            {
                Console.WriteLine("not found");
                return false;
            }
            
            if(!x.UserId.Equals(userId))
            {
                Console.WriteLine("not the user");
                return false;
            }

            if(x.Expiration <= DateTimeOffset.Now)
            {
                Console.WriteLine("refToken expired");
                return false;
            }

            return true;
        }

        public void RemoveRefreshToken(string refreshToken)
        {
            if(string.IsNullOrEmpty(refreshToken))
            {
                return;
            }

            var entity = _context.RefreshTokens.Find(refreshToken);

            if(entity == null)
            {
                return;
            }

            _context.RefreshTokens.Remove(entity);
            _context.SaveChanges();
        }

        public string GetIdByUsername(string username)
        {
            return _context.Users.FirstOrDefault(u => u.UserName.Equals(username)).Id;
        }

        public RefreshToken GetRefreshToken(string userId)
        {
            return new RefreshToken(_context.RefreshTokens.FirstOrDefault(r => r.UserId.Equals(userId)));
        }

        public void SetRefreshToken(string userId, RefreshToken refreshToken)
        {
            if(userId == null)
            {
                return;
            }

            var entity = new RefreshTokenEntity()
            {
                UserId = userId,
                Token = refreshToken.Token,
                Expiration = refreshToken.Expiration
            };

            _context.RefreshTokens.Add(entity);
            Console.WriteLine("Got here with token:"+refreshToken.Token);
            _context.SaveChanges();
        }
        public void SetRefreshToken(string userId, string refreshToken)
        {
            SetRefreshToken(userId,
                new RefreshToken()
                { 
                    Token = refreshToken,
                    Expiration = DateTime.Now.AddMinutes(Int32.Parse(_configuration["JWT:ExpirationTime"]))
                });
        }
    }
}
