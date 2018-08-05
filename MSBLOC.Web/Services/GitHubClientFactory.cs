using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using MSBLOC.Core.Interfaces;
using MSBLOC.Core.Services;
using Octokit;
using CoreGitHubClientFactory = MSBLOC.Core.Services.GitHubClientFactory;
using IGitHubClientFactory = MSBLOC.Web.Interfaces.IGitHubClientFactory;

namespace MSBLOC.Web.Services
{
    public class GitHubClientFactory : CoreGitHubClientFactory, IGitHubClientFactory
    {
        private readonly ITokenGenerator _tokenGenerator;
        private readonly IHttpContextAccessor _contextAccessor;

        public GitHubClientFactory(IHttpContextAccessor contextAccessor, ITokenGenerator tokenGenerator)
        {
            _contextAccessor = contextAccessor;
            _tokenGenerator = tokenGenerator;
        }

        public async Task<IGitHubClient> CreateAppClient(string repositoryOwner)
        {
            return await CreateAppClient(_tokenGenerator, repositoryOwner);
        }

        public async Task<IGitHubClient> CreateClientForCurrentUser()
        {
            var token = await _contextAccessor.HttpContext.GetTokenAsync("access_token");
            return CreateClient(token);
        }
    }
}
