using System;
using System.Linq;
using System.Threading.Tasks;
using BCC.Core.Interfaces.GitHub;
using Octokit;
using IGitHubAppClientFactory = BCC.Web.Interfaces.GitHub.IGitHubAppClientFactory;
using IGitHubGraphQLClient = BCC.Web.Interfaces.GitHub.IGitHubGraphQLClient;
using ITokenGenerator = BCC.Web.Interfaces.GitHub.ITokenGenerator;

namespace BCC.Web.Services.GitHub
{
    /// <inheritdoc />
    public class GitHubAppClientFactory : IGitHubAppClientFactory
    {
        /// <inheritdoc />
        public async Task<IGitHubClient> CreateAppClientForLoginAsync(ITokenGenerator tokenGenerator, string login)
        {
            var (installation, token) = await FindInstallationAndGetToken(tokenGenerator, login);
            return GitHubClientFactoryHelper.GitHubClient(token, GetUserAgent(installation));
        }

        /// <inheritdoc />
        public IGitHubClient CreateAppClient(ITokenGenerator tokenGenerator)
        {
            var token = tokenGenerator.GetToken();

            return new GitHubClient(new ProductHeaderValue("BuildCrossCheck"))
            {
                Credentials = new Credentials(token, AuthenticationType.Bearer)
            };
        }

        /// <inheritdoc />
        public IGitHubGraphQLClient CreateAppGraphQLClient(ITokenGenerator tokenGenerator)
        {
            var token = tokenGenerator.GetToken();
            return GitHubClientFactoryHelper.GraphQLClient(token, GitHubClientFactory.UserAgent);
        }

        /// <inheritdoc />
        public async Task<IGitHubGraphQLClient> CreateAppGraphQLClientForLoginAsync(ITokenGenerator tokenGenerator, string login)
        {
            var (installation, token) = await FindInstallationAndGetToken(tokenGenerator, login);
            return GitHubClientFactoryHelper.GraphQLClient(token, GetUserAgent(installation));
        }

        private async Task<ValueTuple<Installation, string>> FindInstallationAndGetToken(ITokenGenerator tokenGenerator, string login)
        {
            var appClient = CreateAppClient(tokenGenerator);

            var installations = await appClient.GitHubApps.GetAllInstallationsForCurrent();
            var installation = installations.First(inst =>
                string.Equals(inst.Account.Login, login, StringComparison.InvariantCultureIgnoreCase));

            var response = await appClient.GitHubApps.CreateInstallationToken(installation.Id);

            return (installation, response.Token);
        }

        private string GetUserAgent(InstallationId installation) => $"{GitHubClientFactory.UserAgent}-Installation{installation.Id}";
    }
}
