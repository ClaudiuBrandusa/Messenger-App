using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Messenger_API.Models
{
    public class LoginResponse
    {
        public string AccessToken { get; set; }
        public DateTimeOffset AccessTokenExpiration { get; set; }
        public string RefreshToken { get; set; }
    }
}
