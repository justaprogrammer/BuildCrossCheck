using System;
using System.Linq;
using System.Threading.Tasks;
using MSBLOC.Core.Interfaces;
using Octokit;
using Octokit.Internal;

namespace MSBLOC.Core.Services
{
    public class GitHubClientFactory : IGitHubClientFactory
    {
        public GitHubClientFactory()
        {
        }

        public async Task<IGitHubClient> CreateAppClient(ITokenGenerator tokenGenerator, string login)
        {
            var (installation, token) = await FindInstallationAndGetToken(tokenGenerator, login);
            return new GitHubClient(new ProductHeaderValue(GetUserAgent(installation)),
                new InMemoryCredentialStore(new Credentials(token)));
        }

        public async Task<IGitHubGraphQLClient> CreateAppGraphQLClient(ITokenGenerator tokenGenerator, string login)
        {
            var (installation, token) = await FindInstallationAndGetToken(tokenGenerator, login);
            return new GitHubGraphQLClient(new Octokit.GraphQL.ProductHeaderValue(GetUserAgent(installation)), token);
        }

        public IGitHubClient CreateClient(string token)
        {
            return new GitHubClient(new ProductHeaderValue(GetUserAgent()),
                new InMemoryCredentialStore(new Credentials(token)));
        }

        public IGitHubGraphQLClient CreateGraphQLClient(string token)
        {
            return new GitHubGraphQLClient(new Octokit.GraphQL.ProductHeaderValue(GetUserAgent()), token);
        }

        private static string GetUserAgent(InstallationId installation = null)
        {
            const string userAgent = "MSBuildLogOctokitChecker";

            if (installation != null) return $"{userAgent}-Installation{installation.Id}";

            return userAgent;
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
    }
}