using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using MSBLOC.Infrastructure.Models;
using MSBLOC.Web.Interfaces;
using MSBLOC.Web.Models;
using Newtonsoft.Json.Linq;

namespace MSBLOC.Web.Services
{
    public class JsonWebTokenService : IJsonWebTokenService
    {
        private readonly IOptions<AuthOptions> _optionsAccessor;

        public JsonWebTokenService(IOptions<AuthOptions> optionsAccessor)
        {
            _optionsAccessor = optionsAccessor;
        }

        public (AccessToken AccessToken, string JsonWebToken) CreateToken(ClaimsPrincipal user, long githubRepositoryId)
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

            var payload = new JObject()
            {
                { JwtRegisteredClaimNames.Aud, "MSBLOC.Api" },
                { JwtRegisteredClaimNames.Jti, accessToken.Id },
                { JwtRegisteredClaimNames.Iat, accessToken.IssuedAt.ToUnixTimeSeconds() },
                { "urn:msbloc:repositoryId", githubRepositoryId },
                { JwtRegisteredClaimNames.Sub, accessToken.IssuedTo },
            };

            var accessTokenString = tokenHandler.CreateToken(payload, signingCredentials);
            return (accessToken, accessTokenString);
        }

        public TokenValidationResult ValidateToken(string accessToken)
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

            return tokenValidationResult;
        }

        private SecurityKey SecurityKey => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_optionsAccessor.Value.Secret));
    }
}
