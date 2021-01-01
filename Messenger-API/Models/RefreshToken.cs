using Messenger_API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Messenger_API.Models
{
    public class RefreshToken
    {
        public string Token { get; set; }
        public DateTimeOffset Expiration { get; set; }

        public RefreshToken(RefreshTokenEntity entity)
        {
            Token = entity.Token;
            Expiration = entity.Expiration;
        }

        public RefreshToken()
        {
        }
    }
}
