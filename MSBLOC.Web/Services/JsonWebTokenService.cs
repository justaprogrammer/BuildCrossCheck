using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using MSBLOC.Infrastructure.Interfaces;
using MSBLOC.Infrastructure.Models;
using MSBLOC.Web.Interfaces;
using MSBLOC.Web.Models;
using Newtonsoft.Json.Linq;

namespace MSBLOC.Web.Services
{
    public class JsonWebTokenService : IJsonWebTokenService
    {
        private readonly IOptions<AuthOptions> _optionsAccessor;
        private readonly IAccessTokenRepository _tokenRepository;

        public JsonWebTokenService(IOptions<AuthOptions> optionsAccessor, IAccessTokenRepository tokenRepository)
        {
            _optionsAccessor = optionsAccessor;
            _tokenRepository = tokenRepository;
        }

        public async Task<string> CreateTokenAsync(ClaimsPrincipal user, long githubRepositoryId)
        {
            var tokenHandler = new JsonWebTokenHandler();
            var signingCredentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256Signature);

            var accessToken = new AccessToken()
            {
                Id = Guid.NewGuid(),
                GitHubRepositoryId = githubRepositoryId,
                IssuedAt = DateTimeOffset.UtcNow,
                IssuedTo = user.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value
            };

            await _tokenRepository.AddAsync(accessToken);

            var payload = new JObject()
            {
                { JwtRegisteredClaimNames.Aud, "MSBLOC.Api" },
                { JwtRegisteredClaimNames.Jti, accessToken.Id },
                { JwtRegisteredClaimNames.Iat, accessToken.IssuedAt.ToUnixTimeSeconds() },
                { "urn:msbloc:repositoryId", githubRepositoryId },
                { JwtRegisteredClaimNames.Sub, accessToken.IssuedTo },
            };

            var accessTokenString = tokenHandler.CreateToken(payload, signingCredentials);

            return accessTokenString;
        }

        public async Task<JsonWebToken> ValidateTokenAsync(string accessToken)
        {
            var tokenHandler = new JsonWebTokenHandler();
            var tokenValidationParameters = new TokenValidationParameters()
            {
                ValidAudience = "MSBLOC.Api",
                IssuerSigningKey = SecurityKey,
                RequireExpirationTime = false,
                ValidateLifetime = false,
                ValidateIssuer = false
            };

            var tokenValidationResult = tokenHandler.ValidateToken(accessToken, tokenValidationParameters);

            var jwt = tokenValidationResult.SecurityToken as JsonWebToken;

            if (jwt == null) throw new Exception("Invalid token format.");

            await _tokenRepository.GetAsync(new Guid(jwt.Id));

            return jwt;
        }

        private SecurityKey SecurityKey => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_optionsAccessor.Value.Secret));
    }
}
