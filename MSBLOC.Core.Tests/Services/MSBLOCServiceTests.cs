using System;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MSBLOC.Core.Interfaces;
using MSBLOC.Core.Model;
using MSBLOC.Core.Services;
using MSBLOC.Core.Tests.Util;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace MSBLOC.Core.Tests.Services
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
            IGitHubAppModelService gitHubAppModelService = null)
        {
            if (binaryLogProcessor == null) binaryLogProcessor = Substitute.For<IBinaryLogProcessor>();

            if (gitHubAppModelService == null) gitHubAppModelService = Substitute.For<IGitHubAppModelService>();

            return new MSBLOCService(binaryLogProcessor, gitHubAppModelService, TestLogger.Create<MSBLOCService>(_testOutputHelper));
        }

        [Fact]
        public async Task SubmitTest()
        {

            var cloneRoot = Faker.System.DirectoryPath();
            var buildDetails = new BuildDetails(new SolutionDetails(cloneRoot));

            var binaryLogProcessor = Substitute.For<IBinaryLogProcessor>();
            binaryLogProcessor.ProcessLog(null, null, null, null, null).ReturnsForAnyArgs(buildDetails);

            var id = Faker.Random.Long();
            var url = Faker.Internet.Url();

            var gitHubAppModelService = Substitute.For<IGitHubAppModelService>();
            gitHubAppModelService.CreateCheckRunAsync(null, null, null, null, null, null, null, null, null)
                .ReturnsForAnyArgs(new CheckRun()
                {
                    Id = id,
                    Url = url
                });

            var msblocService = CreateTarget(binaryLogProcessor, gitHubAppModelService);

            string repoOwner = Faker.Lorem.Word();
            string repoName = Faker.Lorem.Word();
            string sha = Faker.Random.String();
            string root = Faker.System.DirectoryPath();
            string resourcePath = Faker.System.FilePath();

            var checkRun = await msblocService.SubmitAsync(repoOwner, repoName, sha, root, resourcePath);
            checkRun.Id.Should().Be(id);
            checkRun.Url.Should().Be(url);

            Received.InOrder(async () =>
            {
                binaryLogProcessor.Received(1).ProcessLog(Arg.Is(resourcePath), Arg.Is(root), repoOwner, repoName, sha);
                await gitHubAppModelService.Received(1).CreateCheckRunAsync(
                    Arg.Is(repoOwner),
                    Arg.Is(repoName),
                    Arg.Is(sha),
                    Arg.Any<string>(), 
                    Arg.Any<string>(), 
                    Arg.Any<string>(), 
                    Arg.Any<Annotation[]>(),
                    Arg.Any<DateTimeOffset>(), 
                    Arg.Any<DateTimeOffset>());
            });
        }
    }
}