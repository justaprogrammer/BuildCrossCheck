using System;
using System.Linq;
using System.Threading.Tasks;
using MSBLOC.Core.Interfaces;
using Octokit;

namespace MSBLOC.Core.Services.Factories
{
    public class GitHuAppClientFactory : IGitHuAppClientFactory
    {
        public async Task<IGitHubClient> CreateClient(ITokenGenerator tokenGenerator, string login)
        {
            var (installation, token) = await FindInstallationAndGetToken(tokenGenerator, login);
            return GitHubClientFactoryHelper.GitHubClient(token, GetUserAgent(installation));
        }

        public async Task<IGitHubGraphQLClient> CreateGraphQLClient(ITokenGenerator tokenGenerator, string login)
        {
            var (installation, token) = await FindInstallationAndGetToken(tokenGenerator, login);
            return GitHubClientFactoryHelper.GraphQLClient(token, GetUserAgent(installation));
        }

        private async Task<ValueTuple<Installation, string>> FindInstallationAndGetToken(ITokenGenerator tokenGenerator, string login)
        {
            var jwtToken = tokenGenerator.GetToken();

            var appClient = new GitHubClient(new ProductHeaderValue("MSBuildLogOctokitChecker"))
            {
                Credentials = new Credentials(jwtToken, AuthenticationType.Bearer)
            };

            var installations = await appClient.GitHubApps.GetAllInstallationsForCurrent();
            var installation = installations.First(inst =>
                string.Equals(inst.Account.Login, login, StringComparison.InvariantCultureIgnoreCase));

            var response = await appClient.GitHubApps.CreateInstallationToken(installation.Id);

            return (installation, response.Token);
        }

        private string GetUserAgent(InstallationId installation) => $"{GitHubClientFactory.UserAgent}-Installation{installation.Id}";
    }
}