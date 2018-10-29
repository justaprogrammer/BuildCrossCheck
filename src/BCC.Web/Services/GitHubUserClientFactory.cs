using System.Threading.Tasks;
using BCC.Core.Interfaces.GitHub;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Octokit;
using IGitHubClientFactory = BCC.Web.Interfaces.GitHub.IGitHubClientFactory;
using IGitHubGraphQLClient = BCC.Web.Interfaces.GitHub.IGitHubGraphQLClient;
using IGitHubUserClientFactory = BCC.Web.Interfaces.GitHub.IGitHubUserClientFactory;

namespace BCC.Web.Services
{
    /// <inheritdoc />
    public class GitHubUserClientFactory: IGitHubUserClientFactory
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IGitHubClientFactory _gitHubClientFactory;

        public GitHubUserClientFactory(IHttpContextAccessor contextAccessor, IGitHubClientFactory gitHubClientFactory)
        {
            _contextAccessor = contextAccessor;
            _gitHubClientFactory = gitHubClientFactory;
        }

        /// <inheritdoc />
        public async Task<IGitHubClient> CreateClient()
        {
            var token = await GetAccessToken();
            return _gitHubClientFactory.CreateClient(token);
        }

        /// <inheritdoc />
        public async Task<IGitHubGraphQLClient> CreateGraphQLClient()
        {
            var token = await GetAccessToken();
            return _gitHubClientFactory.CreateGraphQLClient(token);
        }

        private async Task<string> GetAccessToken()
        {
            return await _contextAccessor.HttpContext.GetTokenAsync("access_token");
        }
    }
}
