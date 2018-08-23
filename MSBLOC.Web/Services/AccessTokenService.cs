using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using MSBLOC.Core.Interfaces;
using MSBLOC.Core.Model;
using MSBLOC.Core.Model.GitHub;
using MSBLOC.Infrastructure.Interfaces;
using MSBLOC.Web.Interfaces;
using MSBLOC.Web.Models;
using Newtonsoft.Json.Linq;
using Octokit;
using AccessToken = MSBLOC.Infrastructure.Models.AccessToken;

namespace MSBLOC.Web.Services
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
            UserRepository repository = await _gitHubUserModelService.GetUserRepositoryAsync(githubRepositoryId);

            if (repository == null)
            {
                throw new ArgumentException("Repository does not exist or no permission to access given repository.");
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
                { JwtRegisteredClaimNames.Aud, "MSBLOC.Api" },
                { JwtRegisteredClaimNames.Jti, accessToken.Id },
                { JwtRegisteredClaimNames.Iat, accessToken.IssuedAt.ToUnixTimeSeconds() },
                { "urn:msbloc:repositoryId", repository.Id },
                { "urn:msbloc:repositoryName", repository.Name },
                { "urn:msbloc:repositoryOwner", repository.Owner},
                { "urn:msbloc:repositoryOwnerId", repository.OwnerId },
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

        public async Task RevokeTokenAsync(Guid tokenId)
        {
            var userInstallations = await _gitHubUserModelService.GetUserRepositoriesAsync();
            var repositoryIds = userInstallations.Select(r => r.Id).ToList();

            await _tokenRepository.DeleteAsync(tokenId, repositoryIds);
        }

        public async Task<IEnumerable<AccessToken>> GetTokensForUserRepositoriesAsync()
        {
            var userRepositories = await _gitHubUserModelService.GetUserRepositoriesAsync();
            var repositoryIds = userRepositories.Select(r => r.Id).ToList();

            return await _tokenRepository.GetByRepositoryIdsAsync(repositoryIds);
        }

        private SecurityKey SecurityKey => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_optionsAccessor.Value.Secret));
    }
}
