using System.Threading.Tasks;
using FluentAssertions;
using MSBLOC.Core.IntegrationTests.Utilities;
using MSBLOC.Core.Interfaces;
using MSBLOC.Core.Services;
using Octokit;

namespace MSBLOC.Core.IntegrationTests.Services
{
    public class GitHubAppModelServiceBaseTests : IntegrationTestsBase
    {
        [IntegrationTest]
        public async Task ShouldGetResitoryFile()
        {
            var testAppModelService = CreateTarget();
            var appveyor = await testAppModelService.GetRepositoryFileAsync("justaprogrammer", "MSBuildLogOctokitChecker", "appveyor.yml", "master");
            appveyor.Should().NotBeNull();
        }

        [IntegrationTest]
        public async Task ShouldNotGetFileThatDoesNotExist()
        {
            var testAppModelService = CreateTarget();
            var appveyor = await testAppModelService.GetRepositoryFileAsync("justaprogrammer", "MSBuildLogOctokitChecker", "appveyor2.yml", "master");
            appveyor.Should().BeNull();
        }

        private TestAppModelService CreateTarget()
        {
            return new TestAppModelService(CreateGitHubTokenClient(), CreateGitHubGraphQLTokenClient());
        }

        private class TestAppModelService : GitHubAppModelServiceBase
        {
            private readonly IGitHubClient _gitHubClient;
            private readonly IGitHubGraphQLClient _graphQLClient;

            public TestAppModelService(IGitHubClient gitHubClient, IGitHubGraphQLClient graphQLClient)
            {
                _graphQLClient = graphQLClient;
                _gitHubClient = gitHubClient;
            }

            public Task<string> GetRepositoryFileAsync(string owner, string repository, string filepath, string reference)
            {
                return GetRepositoryFileAsync(_gitHubClient, owner, repository, filepath, reference);
            }
        }
    }
}