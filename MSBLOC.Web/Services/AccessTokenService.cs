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
        private readonly IGitHubUserClientFactory _gitHubUserClientFactory;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IGitHubUserModelService _gitHubUserModelService;

        public AccessTokenService(IOptions<AuthOptions> optionsAccessor, 
            IAccessTokenRepository tokenRepository, 
            IGitHubUserClientFactory gitHubUserClientFactory, 
            IHttpContextAccessor contextAccessor,
            IGitHubUserModelService gitHubUserModelService)
        {
            _optionsAccessor = optionsAccessor;
            _tokenRepository = tokenRepository;
            _gitHubUserClientFactory = gitHubUserClientFactory;
            _contextAccessor = contextAccessor;
            _gitHubUserModelService = gitHubUserModelService;
        }

        public async Task<string> CreateTokenAsync(long githubRepositoryId)
        {
            var github = await _gitHubUserClientFactory.CreateClient();

            var repository = await github.Repository.Get(githubRepositoryId);

            if (repository == null)
            {
                throw new ArgumentException("Repository does not exist or no permission to access given repository.");
            }

            githubRepositoryId = repository.Id;

            var tokenHandler = new JsonWebTokenHandler();
            var signingCredentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256Signature);

            var user = _contextAccessor.HttpContext.User;

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

        public async Task RevokeTokenAsync(Guid tokenId)
        {
            var github = await _gitHubUserClientFactory.CreateClient();

            var repositories = (await github.Repository.GetAllForCurrent()).ToList();

            var repositoryIds = repositories.Select(r => r.Id).ToList();

            await _tokenRepository.DeleteAsync(tokenId, repositoryIds);
        }

        public async Task<IEnumerable<AccessToken>> GetTokensForUserRepositoriesAsync()
        {
            var userInstallations = await _gitHubUserModelService.GetUserInstallations();
            var repositories = userInstallations.SelectMany(installation => installation.Repositories).ToArray();

            var repositoryIds = repositories.Select(r => r.Id).ToList();

            return await _tokenRepository.GetByRepositoryIdsAsync(repositoryIds);
        }

        private SecurityKey SecurityKey => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_optionsAccessor.Value.Secret));
    }
}
