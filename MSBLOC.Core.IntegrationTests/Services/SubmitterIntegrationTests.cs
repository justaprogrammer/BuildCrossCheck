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
            const string sha = "9ebabb7dad7f93ebd947fb005175c57346bed21a";

            await AssertSubmit(file, sha);
        }

        [IntegrationTest]
        public async Task ShouldSubmitError()
        {
            const string file = "testconsoleapp1-1error.binlog";
            const string sha = "d12f68584e84d3e885079e322c8d8bd11cfa1f58";

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
            var parser = new Parser(TestLogger.Create<Parser>(_testOutputHelper));
            var buildDetails = parser.Parse(resourcePath, cloneRoot);

            var privateKeySource = new EnvironmentVariablePrivateKeySource(privateKeyEnvironmentVariableName);
            var tokenGenerator = new TokenGenerator(gitHubAppId, privateKeySource);
            var gitHubClientFactory = new GitHubClientFactory(tokenGenerator);
            var gitHubClient = await gitHubClientFactory.CreateClientForLogin(integrationTestAppOwner);
            var checkRunsClient = gitHubClient.Check.Run;

            var submitter = new Submitter(checkRunsClient);
            var checkRun = await submitter.SubmitCheckRun(
                owner: integrationTestAppOwner,
                name: integrationTestAppName,
                headSha: sha,
                checkRunName: "MSBuildLog Analyzer",
                buildDetails: buildDetails,
                checkRunTitle: "MSBuildLog Analysis",
                checkRunSummary: "",
                startedAt: startedAt,
                completedAt: DateTimeOffset.Now,
                cloneRoot: cloneRoot);

            checkRun.Should().NotBeNull();

            _logger.LogInformation($"CheckRun Created - {checkRun.HtmlUrl}");
        }
    }
}