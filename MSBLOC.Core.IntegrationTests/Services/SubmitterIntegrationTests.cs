using System.Linq;
using System.Threading.Tasks;
using GitHubJwt;
using Microsoft.Extensions.Logging;
using MSBLOC.Core.IntegrationTests.Utilities;
using MSBLOC.Core.Model;
using MSBLOC.Core.Services;
using MSBLOC.Core.Tests.Util;
using Octokit;
using Xunit.Abstractions;

namespace MSBLOC.Core.IntegrationTests.Services
{
    public class SubmitterIntegrationTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ILogger<SubmitterIntegrationTests> _logger;

        public SubmitterIntegrationTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _logger = TestLogger.Create<SubmitterIntegrationTests>(testOutputHelper);
        }

        [IntegrationTest]
        public async Task ShouldSubmit()
        {
            var environmentVariablePrivateKeySource = new EnvironmentVariablePrivateKeySource(Helper.GitHubAppPrivateKeyEnvironmentVariable);
            var tokenGenerator = new TokenGenerator(Helper.GitHubAppId, environmentVariablePrivateKeySource);
            var jwtToken = tokenGenerator.GetToken();

            var appClient = new GitHubClient(new ProductHeaderValue("MSBuildLogOctokitChecker"))
            {
                Credentials = new Credentials(jwtToken, AuthenticationType.Bearer)
            };

            var gitHubAppsClient = appClient.GitHubApps;
            var current = await gitHubAppsClient.GetCurrent();

            var allInstallationsForCurrent = await gitHubAppsClient.GetAllInstallationsForCurrent();
            var installation = allInstallationsForCurrent.First();

            appClient.GitHubApps.

//            var gitHubCommit = await appClient.Repository.Commit.Get("justaprogrammer", "TestConsoleApp1", "9ebabb7dad7f93ebd947fb005175c57346bed21a");

//            var resourcePath = TestUtils.GetResourcePath("testconsoleapp1-1warning.binlog");
//
//            var parser = new Parser(TestLogger.Create<Parser>(_testOutputHelper));
//            var parsedBinaryLog = parser.Parse(resourcePath);
//
//            var submitter = new Submitter(appClient.Repository.Commit, appClient.Check.Run);
//            var headSha = "9ebabb7dad7f93ebd947fb005175c57346bed21a";
//
//            await submitter.SubmitCheckRun(Helper.IntegrationTestAppOwner, Helper.IntegrationTestAppName, headSha: headSha, checkRunName: "MSBuildLog Analyzer",
//                parsedBinaryLog: parsedBinaryLog, checkRunTitle: "MSBuildLog Anaysis", checkRunSummary: "");
        }
    }
}