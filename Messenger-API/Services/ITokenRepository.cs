using Messenger_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Messenger_API.Services
{
    public interface ITokenRepository
    {
        bool ExistRefreshToken(string refreshToken);
        RefreshToken GetRefreshToken(string userId);
        void SetRefreshToken(string userId, RefreshToken refreshToken);
        void SetRefreshToken(string userId, string refreshToken);
        bool IsRefreshTokenOwnedByUserId(string userId, string refreshToken);
        void RemoveRefreshToken(string refreshToken);
    }
}
