using System;
using System.Linq;
using System.Threading.Tasks;
using MSBLOC.Core.Interfaces;
using Octokit;

namespace MSBLOC.Core.Services.Factories
{
    public class GitHubAppClientFactory : IGitHubAppClientFactory
    {
        public async Task<IGitHubClient> CreateAppClientForLogin(ITokenGenerator tokenGenerator, string login)
        {
            var (installation, token) = await FindInstallationAndGetToken(tokenGenerator, login);
            return GitHubClientFactoryHelper.GitHubClient(token, GetUserAgent(installation));
        }

        public IGitHubClient CreateAppClient(ITokenGenerator tokenGenerator)
        {
            var token = tokenGenerator.GetToken();

            var appClient = new GitHubClient(new ProductHeaderValue("MSBuildLogOctokitChecker"))
            {
                Credentials = new Credentials(token, AuthenticationType.Bearer)
            };

            return appClient;
        }

        public IGitHubGraphQLClient CreateAppGraphQLClient(ITokenGenerator tokenGenerator)
        {
            var token = tokenGenerator.GetToken();
            return GitHubClientFactoryHelper.GraphQLClient(token, GitHubClientFactory.UserAgent);
        }

        public async Task<IGitHubGraphQLClient> CreateAppGraphQLClientForLogin(ITokenGenerator tokenGenerator, string login)
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
