using System.Threading.Tasks;
using BCC.Core.IntegrationTests.Utilities;
using BCC.Core.Interfaces.GitHub;
using BCC.Core.Services.GitHub;
using FluentAssertions;

namespace BCC.Core.IntegrationTests.Services
{
    public class GitHubAppModelServiceIntegrationTests : IntegrationTestsBase
    {
        [IntegrationTest]
        public async Task ShouldGetRepositoryFile()
        {
            var gitHubAppModelService = CreateGitHubAppModelService();
            var content = await gitHubAppModelService.GetRepositoryFileAsync("justaprogrammer", "TestConsoleApp1", "appveyor.yml", "appveyor");
            content.Should().NotBeNull();
        }

        [IntegrationTest]
        public async Task ShouldNotGetRepositoryFileThatDoesNotExist()
        {
            var gitHubAppModelService = CreateGitHubAppModelService();
            var content = await gitHubAppModelService.GetRepositoryFileAsync("justaprogrammer", "TestConsoleApp1", "appveyor2.yml", "appveyor");
            content.Should().BeNull();
        }

        private IGitHubAppModelService CreateGitHubAppModelService()
        {
            var gitHubClientFactory = CreateGitHubAppClientFactory();
            var tokenGenerator = CreateTokenGenerator();
            return new GitHubAppModelService(gitHubClientFactory, tokenGenerator);
        }
    }
}