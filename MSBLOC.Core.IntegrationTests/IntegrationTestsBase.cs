using System.Threading.Tasks;
using GitHubJwt;
using MSBLOC.Core.IntegrationTests.Utilities;
using MSBLOC.Core.Services;
using MSBLOC.Core.Services.Factories;
using Octokit;

namespace MSBLOC.Core.IntegrationTests
{
    public class IntegrationTestsBase
    {
        protected string GitHubAppPrivateKeyEnvironmentVariable { get; } = Helper.GitHubAppPrivateKeyEnvironmentVariable;
        protected int GitHubAppId { get; } = Helper.GitHubAppId;

        protected string TestAppOwner { get; } = Helper.IntegrationTestAppOwner;
        protected string TestAppRepo { get; } = Helper.IntegrationTestAppRepo;
        protected string TestAppInstallationId { get; } = Helper.IntegrationTestAppInstallationId;
        protected string TestToken { get; } = Helper.IntegrationTestToken;
        protected string TestUsername { get; } = Helper.IntegrationTestUsername;

        protected IGitHubClient CreateGitHubAppClient()
        {
            var privateKeySource = new EnvironmentVariablePrivateKeySource(GitHubAppPrivateKeyEnvironmentVariable);
            var tokenGenerator = new TokenGenerator(GitHubAppId, privateKeySource);
            var gitHubClientFactory = new GitHubAppClientFactory();
            return gitHubClientFactory.CreateAppClient(tokenGenerator);
        }

        protected IGitHubGraphQLClient CreateGitHubAppGraphQLClient()
        {
            var privateKeySource = new EnvironmentVariablePrivateKeySource(GitHubAppPrivateKeyEnvironmentVariable);
            var tokenGenerator = new TokenGenerator(GitHubAppId, privateKeySource);
            var gitHubClientFactory = new GitHubAppClientFactory();
            return gitHubClientFactory.CreateAppGraphQLClient(tokenGenerator);
        }

        protected async Task<IGitHubClient> CreateGitHubAppClientForLogin(string login)
        {
            var privateKeySource = new EnvironmentVariablePrivateKeySource(GitHubAppPrivateKeyEnvironmentVariable);
            var tokenGenerator = new TokenGenerator(GitHubAppId, privateKeySource);
            var gitHubClientFactory = new GitHubAppClientFactory();
            return await gitHubClientFactory.CreateAppClientForLogin(tokenGenerator, login);
        }

        protected async Task<IGitHubGraphQLClient> CreateGitHubAppGraphQLClientForLogin(string login)
        {
            var privateKeySource = new EnvironmentVariablePrivateKeySource(GitHubAppPrivateKeyEnvironmentVariable);
            var tokenGenerator = new TokenGenerator(GitHubAppId, privateKeySource);
            var gitHubClientFactory = new GitHubAppClientFactory();
            return await gitHubClientFactory.CreateAppGraphQLClientForLogin(tokenGenerator, login);
        }

        protected IGitHubClient CreateGitHubUserClient()
        {
            var gitHubClientFactory = new GitHubClientFactory();
            return gitHubClientFactory.CreateClient(TestToken);
        }

        protected IGitHubGraphQLClient CreateGitHubQLClient()
        {
            var gitHubClientFactory = new GitHubClientFactory();
            return gitHubClientFactory.CreateGraphQLClient(TestToken);
        }
    }
}