using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using MSBLOC.Core.Interfaces;
using MSBLOC.Core.Services;
using MSBLOC.Web.Interfaces;
using Octokit;

namespace MSBLOC.Web.Services
{
    public class GitHubUserClientFactory: IGitHubUserClientFactory
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IGitHubClientFactory _gitHubClientFactory;

        public GitHubUserClientFactory(IHttpContextAccessor contextAccessor, IGitHubClientFactory gitHubClientFactory)
        {
            _contextAccessor = contextAccessor;
            _gitHubClientFactory = gitHubClientFactory;
        }

        public async Task<IGitHubClient> CreateClient()
        {
            var token = await GetAccessToken();
            return _gitHubClientFactory.CreateClient(token);
        }

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
