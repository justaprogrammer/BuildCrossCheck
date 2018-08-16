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
        public async Task ShouldGetPullRequestChangedPaths()
        {
            var testAppModelService = CreateTarget();
            var paths = await testAppModelService.GetPullRequestChangedPaths("octokit", "octokit.graphql.net", 142);
            paths.Length.Should().Be(57);
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

            public async Task<string[]> GetPullRequestChangedPaths(string owner, string name, int pullRequest)
            {
                return await GetPullRequestChangedPaths(_gitHubClient, owner, name, pullRequest);
            }
        }
    }
}