using System.Threading.Tasks;
using BCC.Web.IntegrationTests.Utilities;

namespace BCC.Web.IntegrationTests.Services
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