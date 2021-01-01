using Messenger_API.Authentication;
using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Messenger_API.Entities
{
    public class RefreshTokenEntity
    {
        public string UserId { get; set; }
        [Key]
        public string Token { get; set; }
        public DateTimeOffset Expiration { get; set; }
    }
}
