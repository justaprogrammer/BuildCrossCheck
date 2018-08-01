using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
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
        public async Task ShouldSubmitWarning()
        {
            const string file = "testconsoleapp1-1warning.binlog";
            const string sha = "7d8c017ce2934453a2166f28e4eb3b92640c97e4";

            await AssertSubmit(file, sha);
        }

        [IntegrationTest]
        public async Task ShouldSubmitError()
        {
            const string file = "testconsoleapp1-1error.binlog";
            const string sha = "478882ff771853355906ea1e7177daa123116aeb";

            await AssertSubmit(file, sha);
        }

        private async Task AssertSubmit(string file, string sha)
        {
            var gitHubAppId = Helper.GitHubAppId;
            var integrationTestAppOwner = Helper.IntegrationTestAppOwner;
            var integrationTestAppName = Helper.IntegrationTestAppName;
            var privateKeyEnvironmentVariableName = Helper.GitHubAppPrivateKeyEnvironmentVariable;

            var resourcePath = TestUtils.GetResourcePath(file);
            var cloneRoot = @"C:\projects\testconsoleapp1\";

            var startedAt = DateTimeOffset.Now;
            var parser = new BinaryLogProcessor(TestLogger.Create<BinaryLogProcessor>(_testOutputHelper));
            var buildDetails = parser.ProcessLog(resourcePath, cloneRoot);

            var privateKeySource = new EnvironmentVariablePrivateKeySource(privateKeyEnvironmentVariableName);
            var tokenGenerator = new TokenGenerator(gitHubAppId, privateKeySource);
            var gitHubClientFactory = new GitHubClientFactory(tokenGenerator);
            var gitHubClient = await gitHubClientFactory.CreateClientForLogin(integrationTestAppOwner);
            var checkRunsClient = gitHubClient.Check.Run;

            var submitter = new CheckRunSubmitter(checkRunsClient);
            var checkRun = await submitter.SubmitCheckRun(buildDetails: buildDetails,
                owner: integrationTestAppOwner,
                name: integrationTestAppName,
                headSha: sha,
                checkRunName: "MSBuildLog Analyzer",
                checkRunTitle: "MSBuildLog Analysis",
                checkRunSummary: "",
                startedAt: startedAt,
                completedAt: DateTimeOffset.Now);

            checkRun.Should().NotBeNull();

            _logger.LogInformation($"CheckRun Created - {checkRun.HtmlUrl}");
        }
    }
}