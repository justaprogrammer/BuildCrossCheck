using System.Threading.Tasks;
using MSBLOC.Core.IntegrationTests.Utilities;
using MSBLOC.Core.Interfaces;
using MSBLOC.Core.Interfaces.GitHub;
using MSBLOC.Core.Services;
using MSBLOC.Core.Services.GitHub;

namespace MSBLOC.Core.IntegrationTests.Services
{
    public class GitHubAppModelServiceIntegrationTests : IntegrationTestsBase
    {
        [IntegrationTest]
        public async Task ShouldConstruct()
        {
            var gitHubAppModelService = CreateGitHubAppModelService();
            await gitHubAppModelService.GetPullRequestChangedPathsAsync("justaprogrammer", "TestConsoleApp1", 1);
        }

        private IGitHubAppModelService CreateGitHubAppModelService()
        {
            var gitHubClientFactory = CreateGitHubAppClientFactory();
            var tokenGenerator = CreateTokenGenerator();
            return new GitHubAppModelService(gitHubClientFactory, tokenGenerator);
        }
    }
}