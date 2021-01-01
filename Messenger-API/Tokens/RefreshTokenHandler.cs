using Messenger_API.Models;
using Messenger_API.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Messenger_API.Tokens
{
    public class RefreshTokenHandler : ITokenHandler
    {
        private readonly ITokenRepository _tokenRepository;
        private readonly IConfiguration _configuration;

        public RefreshTokenHandler(ITokenRepository tokenRepository, IConfiguration configuration)
        {
            _tokenRepository = tokenRepository;
            _configuration = configuration;
        }

        public RefreshToken GenerateRefreshToken(string userId)
        {
            string token = GenerateRefreshToken();
            while (_tokenRepository.ExistRefreshToken(token))
            {
                token = GenerateRefreshToken();
            }
            RefreshToken refreshToken = new RefreshToken
            {
                Token = token,
                Expiration = DateTime.Now.AddMinutes(Int32.Parse(_configuration["JWT:ExpirationTime"])+50)
            };

            _tokenRepository.SetRefreshToken(userId, refreshToken);

            return refreshToken;
        }

        string GenerateRefreshToken()
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 300)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public bool IsTokenAvailableForUserId(string userId, string token)
        {
            if(string.IsNullOrEmpty(token))
            {
                return false;
            }

            return _tokenRepository.IsRefreshTokenOwnedByUserId(userId, token);
        }

        public string WriteToken(JwtSecurityToken token)
        {
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public JwtSecurityToken GenerateToken(IEnumerable<Claim> claims)
        {
            var authSigninKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:ApiKey"]));
            var token = new JwtSecurityToken
            (
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddMinutes(Int32.Parse(_configuration["JWT:ExpirationTime"])),
                claims: claims,
                signingCredentials: new SigningCredentials(authSigninKey, SecurityAlgorithms.HmacSha256)
            );

            return token;
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:ApiKey"])),
                ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            if (!tokenHandler.CanReadToken(token))
            {
                return null;
            }
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                Console.WriteLine("Invalid token");
                return null;
            }    
                //throw new SecurityTokenException("Invalid token");

            return principal;
        }
    }
}
