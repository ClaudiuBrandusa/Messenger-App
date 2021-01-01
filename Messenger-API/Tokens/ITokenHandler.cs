using Messenger_API.Models;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Messenger_API.Tokens
{
    public interface ITokenHandler
    {
        JwtSecurityToken GenerateToken(IEnumerable<Claim> claims);
        string WriteToken(JwtSecurityToken token);
        RefreshToken GenerateRefreshToken(string userId);
        bool IsTokenAvailableForUserId(string userId, string token);
        string GenerateRefreshToken();

        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
