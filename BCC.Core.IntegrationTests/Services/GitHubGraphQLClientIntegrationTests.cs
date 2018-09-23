namespace BCC.Core.IntegrationTests.Services
{
    public class GitHubGraphQLClientIntegrationTests : IntegrationTestsBase
    {
        [IntegrationTest]
        public async Task ShouldConstruct()
        {
            var gitHubGraphQLClient = CreateGitHubGraphQLTokenClient();
        }
    }
}