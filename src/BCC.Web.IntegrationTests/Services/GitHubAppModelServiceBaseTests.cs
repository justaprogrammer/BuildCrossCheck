using System.Threading.Tasks;
using BCC.Web.IntegrationTests.Utilities;
using BCC.Web.Interfaces.GitHub;
using BCC.Web.Services.GitHub;
using FluentAssertions;
using Octokit;

namespace BCC.Web.IntegrationTests.Services
{
    public class GitHubAppModelServiceBaseTests : IntegrationTestsBase
    {
        [IntegrationTest]
        public async Task ShouldGetResitoryFile()
        {
            var testAppModelService = CreateTarget();
            var appveyor = await testAppModelService.GetRepositoryFileAsync("justaprogrammer", "BuildCrossCheck", "appveyor.yml", "master");
            appveyor.Should().NotBeNull();
        }

        [IntegrationTest]
        public async Task ShouldNotGetFileThatDoesNotExist()
        {
            var testAppModelService = CreateTarget();
            var appveyor = await testAppModelService.GetRepositoryFileAsync("justaprogrammer", "BuildCrossCheck", "appveyor2.yml", "master");
            appveyor.Should().BeNull();
        }

        private TestAppModelService CreateTarget()
        {
            return new TestAppModelService(CreateGitHubTokenClient());
        }

        private class TestAppModelService : GitHubAppModelServiceBase
        {
            private readonly IGitHubClient _gitHubClient;

            public TestAppModelService(IGitHubClient gitHubClient)
            {
                _gitHubClient = gitHubClient;
            }

            public Task<string> GetRepositoryFileAsync(string owner, string repository, string filepath, string reference)
            {
                return GetRepositoryFileAsync(_gitHubClient, owner, repository, filepath, reference);
            }
        }
    }
}