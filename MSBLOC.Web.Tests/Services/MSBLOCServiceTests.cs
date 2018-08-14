using System;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MSBLOC.Core.Interfaces;
using MSBLOC.Core.Model;
using MSBLOC.Web.Interfaces;
using MSBLOC.Web.Models;
using MSBLOC.Web.Services;
using MSBLOC.Web.Tests.Util;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace MSBLOC.Web.Tests.Services
{
    public class MSBLOCServiceTests
    {
        public MSBLOCServiceTests(ITestOutputHelper testOutputHelper)
        {
            _logger = TestLogger.Create<MSBLOCServiceTests>(testOutputHelper);
            _testOutputHelper = testOutputHelper;
        }

        static MSBLOCServiceTests()
        {
            Faker = new Faker();
        }

        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ILogger<MSBLOCServiceTests> _logger;

        private static readonly Faker Faker;

        private MSBLOCService CreateTarget(
            IBinaryLogProcessor binaryLogProcessor = null,
            IGitHubAppModelService gitHubAppModelService = null,
            ITempFileService tempFileService = null)
        {
            if (binaryLogProcessor == null) binaryLogProcessor = Substitute.For<IBinaryLogProcessor>();

            if (gitHubAppModelService == null) gitHubAppModelService = Substitute.For<IGitHubAppModelService>();

            if (tempFileService == null) tempFileService = Substitute.For<ITempFileService>();

            return new MSBLOCService(binaryLogProcessor, gitHubAppModelService, tempFileService,
                TestLogger.Create<MSBLOCService>(_testOutputHelper));
        }

        [Fact]
        public async Task SubmitTest()
        {

            var cloneRoot = Faker.System.DirectoryPath();
            var buildDetails = new BuildDetails(new SolutionDetails(cloneRoot));

            var binaryLogProcessor = Substitute.For<IBinaryLogProcessor>();
            binaryLogProcessor.ProcessLog(null, null).ReturnsForAnyArgs(buildDetails);

            var tempBinaryLogFilePath = Faker.System.FilePath();
            var tempFileService = Substitute.For<ITempFileService>();
            tempFileService.GetFilePath(null).ReturnsForAnyArgs(tempBinaryLogFilePath);

            var id = Faker.Random.Long();
            var url = Faker.Internet.Url();

            var gitHubAppModelService = Substitute.For<IGitHubAppModelService>();
            gitHubAppModelService.CreateCheckRun(null, null, null, null, null, null, null, null, null)
                .ReturnsForAnyArgs(new CheckRun()
                {
                    Id = id,
                    Url = url
                });

            var msblocService = CreateTarget(binaryLogProcessor, gitHubAppModelService, tempFileService);

            var submissionData = new SubmissionData()
            {
                BinaryLogFile = Faker.System.FilePath()
            };

            var checkRun = await msblocService.SubmitAsync(submissionData);
            checkRun.Id.Should().Be(id);
            checkRun.Url.Should().Be(url);

            Received.InOrder(async () =>
            {
                tempFileService.GetFilePath(submissionData.BinaryLogFile);
                binaryLogProcessor.ProcessLog(Arg.Is(tempBinaryLogFilePath), Arg.Is(submissionData.CloneRoot));
                await gitHubAppModelService.CreateCheckRun(
                    Arg.Is(submissionData.RepoOwner),
                    Arg.Is(submissionData.RepoName),
                    Arg.Is(submissionData.CommitSha),
                    Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Annotation[]>(), Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>());
            });
        }
    }
}