using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MSBLOC.Core.IntegrationTests.Utilities;
using MSBLOC.Core.Services;
using MSBLOC.Core.Tests.Util;
using Xunit.Abstractions;

namespace MSBLOC.Core.IntegrationTests.Concepts
{
    public class MSBLOCServiceIntegrationTests : IntegrationTestsBase
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ILogger<MSBLOCServiceIntegrationTests> _logger;

        public MSBLOCServiceIntegrationTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _logger = TestLogger.Create<MSBLOCServiceIntegrationTests>(testOutputHelper);
        }

        [IntegrationTest]
        public async Task ShouldSubmitWarning()
        {
            const string file = "testconsoleapp1-1warning.binlog";
            const string sha = "b6e4d0c605b79b1a87945cc081e3f728d3100b31";

            await AssertSubmit(file, sha);
        }

        [IntegrationTest]
        public async Task ShouldSubmitError()
        {
            const string file = "testconsoleapp1-1error.binlog";
            const string sha = "53e93a597fd50825ba6060f5bae0d594be714088";

            await AssertSubmit(file, sha);
        }

        private async Task AssertSubmit(string file, string sha)
        {
            var resourcePath = TestUtils.GetResourcePath(file);
            var cloneRoot = @"C:\projects\testconsoleapp1\";

            var startedAt = DateTimeOffset.Now;
            var parser = new BinaryLogProcessor(TestLogger.Create<BinaryLogProcessor>(_testOutputHelper));
            var buildDetails = parser.ProcessLog(resourcePath, cloneRoot, TestAppOwner, TestAppRepo, sha);

            var gitHubAppClientFactory = CreateGitHubAppClientFactory();
            var gitHubAppModelService = new GitHubAppModelService(gitHubAppClientFactory, CreateTokenGenerator());

            var msblocService = new MSBLOCService(parser, gitHubAppModelService, TestLogger.Create<MSBLOCService>(_testOutputHelper));
            var checkRun = await msblocService.SubmitAsync(TestAppOwner, TestAppRepo, sha, cloneRoot, resourcePath);

            checkRun.Should().NotBeNull();
        }
    }
}