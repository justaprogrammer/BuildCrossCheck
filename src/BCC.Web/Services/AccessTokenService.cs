using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BCC.Core.Interfaces.GitHub;
using BCC.Infrastructure.Interfaces;
using BCC.Web.Interfaces;
using BCC.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AccessToken = BCC.Infrastructure.Models.AccessToken;
using IGitHubUserModelService = BCC.Web.Interfaces.GitHub.IGitHubUserModelService;
using Repository = BCC.Web.Models.GitHub.Repository;

namespace BCC.Web.Services
{
    public class AccessTokenService : IAccessTokenService
    {
        private readonly IOptions<AuthOptions> _optionsAccessor;
        private readonly IAccessTokenRepository _tokenRepository;
        private readonly IGitHubUserModelService _gitHubUserModelService;
        private readonly IHttpContextAccessor _contextAccessor;

        public AccessTokenService(IOptions<AuthOptions> optionsAccessor, 
            IAccessTokenRepository tokenRepository,
            IGitHubUserModelService gitHubUserModelService, 
            IHttpContextAccessor contextAccessor)
        {
            _optionsAccessor = optionsAccessor;
            _tokenRepository = tokenRepository;
            _gitHubUserModelService = gitHubUserModelService;
            _contextAccessor = contextAccessor;
        }

        public async Task<string> CreateTokenAsync(long githubRepositoryId)
        {
            Repository repository = await _gitHubUserModelService.GetRepositoryAsync(githubRepositoryId);

            if (repository == null)
            {
                throw new ArgumentException("Repository does not exist or no permission to access given repository.");
            }

            var accessTokens = await _tokenRepository.GetByRepositoryIdsAsync(new[] {githubRepositoryId});
            if (accessTokens.Any())
            {
                throw new ArgumentException("Repository already has a token.");
            }

            var tokenHandler = new JsonWebTokenHandler();
            var signingCredentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256Signature);

            var user = _contextAccessor.HttpContext.User;

            var accessToken = new AccessToken()
            {
                Id = Guid.NewGuid(),
                GitHubRepositoryId = repository.Id,
                IssuedAt = DateTimeOffset.UtcNow,
                IssuedTo = user.Claims.First(c => c.Type.Equals(ClaimTypes.NameIdentifier)).Value
            };

            await _tokenRepository.AddAsync(accessToken);

            var payload = new JObject()
            {
                { JwtRegisteredClaimNames.Aud, ".Api" },
                { JwtRegisteredClaimNames.Jti, accessToken.Id },
                { JwtRegisteredClaimNames.Iat, accessToken.IssuedAt.ToUnixTimeSeconds() },
                { "urn:bcc:repositoryId", repository.Id },
                { "urn:bcc:repositoryName", repository.Name },
                { "urn:bcc:repositoryOwner", repository.Owner},
                { "urn:bcc:repositoryOwnerId", repository.OwnerId },
                { JwtRegisteredClaimNames.Sub, accessToken.IssuedTo },
            };

            return tokenHandler.CreateToken(payload.ToString(Formatting.None), signingCredentials);
        }

        public async Task<JsonWebToken> ValidateTokenAsync(string accessToken)
        {
            var tokenHandler = new JsonWebTokenHandler();
            var tokenValidationParameters = new TokenValidationParameters()
            {
                ValidAudience = ".Api",
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

        public async Task RevokeTokenAsync(Guid tokenId)
        {
            var userInstallations = await _gitHubUserModelService.GetRepositoriesAsync();
            var repositoryIds = userInstallations.Select(r => r.Id).ToList();

            await _tokenRepository.DeleteAsync(tokenId, repositoryIds);
        }

        public async Task<IEnumerable<AccessToken>> GetTokensForUserRepositoriesAsync()
        {
            var userRepositories = await _gitHubUserModelService.GetRepositoriesAsync();
            var repositoryIds = userRepositories.Select(r => r.Id).ToList();

            return await _tokenRepository.GetByRepositoryIdsAsync(repositoryIds);
        }

        private SecurityKey SecurityKey => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_optionsAccessor.Value.Secret));
    }
}
