using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace Functions.SignalR.Services
{
    public interface ITokenService
    {
        string GenerateClientAccessToken(string hubName, string userId = null);
        string GenerateServerAccessToken(string url);
    }
    public class TokenService : ITokenService
    {
        private readonly JwtSecurityTokenHandler _JwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        private readonly TimeSpan _Duration;
        private readonly bool _RequireIdentity;
        private readonly string _AccessKey;
        private readonly string _Endpoint;

        public TokenService(string endpoint, string accessKey, TimeSpan duration, bool requireIdentity = true)
        {
            _Endpoint = endpoint;
            _AccessKey = accessKey;
            _Duration = duration;
            _RequireIdentity = requireIdentity;
        }

        /// <summary>
        /// Generate a Client token for the SignalR Service
        /// </summary>
        public string GenerateClientAccessToken(string hubName, string userId = null)
        {
            Claim identity = null;

            if (_RequireIdentity && string.IsNullOrEmpty(userId)) { throw new ArgumentNullException("UserId must be provided"); }
            else
            {
                identity = new Claim(ClaimTypes.NameIdentifier, userId);
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_AccessKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = _JwtSecurityTokenHandler.CreateJwtSecurityToken(
                issuer: null,
                audience: $"{_Endpoint}/client/?hub={hubName}",
                subject: identity != null ? new ClaimsIdentity(new[] { identity }) : null,
                expires: DateTime.UtcNow.Add(_Duration),
                signingCredentials: credentials);
            return _JwtSecurityTokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Generate a Server token for the SignalR Service
        /// </summary>
        public string GenerateServerAccessToken(string url)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_AccessKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = _JwtSecurityTokenHandler.CreateJwtSecurityToken(
                issuer: null,
                audience: url,
                subject: null,
                expires: DateTime.UtcNow.Add(_Duration),
                signingCredentials: credentials);
            return _JwtSecurityTokenHandler.WriteToken(token);
        }
    }
}

