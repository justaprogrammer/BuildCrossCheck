using System.Threading.Tasks;
using BCC.Web.IntegrationTests.Utilities;
using BCC.Web.Interfaces.GitHub;
using BCC.Web.Services.GitHub;
using GitHubJwt;
using Octokit;

namespace BCC.Web.IntegrationTests
{
    public class IntegrationTestsBase
    {
        protected string GitHubAppPrivateKeyEnvironmentVariable { get; } =
            Helper.GitHubAppPrivateKeyEnvironmentVariable;

        protected int GitHubAppId { get; } = Helper.GitHubAppId;

        protected string TestAppOwner { get; } = Helper.IntegrationTestAppOwner;
        protected string TestAppRepo { get; } = Helper.IntegrationTestAppRepo;
        protected long TestAppInstallationId { get; } = long.Parse(Helper.IntegrationTestAppInstallationId);
        protected string TestToken { get; } = Helper.IntegrationTestToken;
        protected string TestUsername { get; } = Helper.IntegrationTestUsername;

        protected IGitHubAppClientFactory CreateGitHubAppClientFactory()
        {
            return new GitHubAppClientFactory();
        }

        protected TokenGenerator CreateTokenGenerator()
        {
            var privateKeySource = new EnvironmentVariablePrivateKeySource(GitHubAppPrivateKeyEnvironmentVariable);
            return new TokenGenerator(GitHubAppId, privateKeySource);
        }

        protected IGitHubClient CreateGitHubAppClient()
        {
            var gitHubClientFactory = CreateGitHubAppClientFactory();
            var tokenGenerator = CreateTokenGenerator();
            return gitHubClientFactory.CreateAppClient(tokenGenerator);
        }

        protected IGitHubGraphQLClient CreateGitHubAppGraphQLClient()
        {
            var gitHubClientFactory = CreateGitHubAppClientFactory();
            var tokenGenerator = CreateTokenGenerator();
            return gitHubClientFactory.CreateAppGraphQLClient(tokenGenerator);
        }

        protected async Task<IGitHubClient> CreateGitHubAppClientForLogin(string login)
        {
            var gitHubClientFactory = CreateGitHubAppClientFactory();
            var tokenGenerator = CreateTokenGenerator();
            return await gitHubClientFactory.CreateAppClientForLoginAsync(tokenGenerator, login);
        }

        protected async Task<IGitHubGraphQLClient> CreateGitHubAppGraphQLClientForLogin(string login)
        {
            var gitHubClientFactory = CreateGitHubAppClientFactory();
            var tokenGenerator = CreateTokenGenerator();
            return await gitHubClientFactory.CreateAppGraphQLClientForLoginAsync(tokenGenerator, login);
        }

        protected IGitHubClient CreateGitHubTokenClient()
        {
            var gitHubClientFactory = new GitHubClientFactory();
            return gitHubClientFactory.CreateClient(TestToken);
        }

        protected IGitHubGraphQLClient CreateGitHubGraphQLTokenClient()
        {
            var gitHubClientFactory = new GitHubClientFactory();
            return gitHubClientFactory.CreateGraphQLClient(TestToken);
        }
    }
}