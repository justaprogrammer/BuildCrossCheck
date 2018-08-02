using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MSBLOC.Core.Interfaces;
using MSBLOC.Core.Model;
using MSBLOC.Web.Interfaces;
using MSBLOC.Web.Models;
using MSBLOC.Web.Services;
using MSBLOC.Web.Tests.Controllers.api;
using MSBLOC.Web.Tests.Util;
using NSubstitute;
using Octokit;
using Xunit;
using Xunit.Abstractions;

namespace MSBLOC.Web.Tests.Services
{
    public class MSBLOCServiceTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ILogger<MSBLOCServiceTests> _logger;

        public MSBLOCServiceTests(ITestOutputHelper testOutputHelper)
        {
            _logger = TestLogger.Create<MSBLOCServiceTests>(testOutputHelper);
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public async Task SubmitTest()
        {
            var submissionData = new Faker<SubmissionData>()
                .RuleFor(sd => sd.RepoOwner, f => f.Person.FullName)
                .RuleFor(sd => sd.RepoName, f => f.Hacker.Phrase())
                .RuleFor(sd => sd.CloneRoot, f => f.System.DirectoryPath())
                .RuleFor(sd => sd.CommitSha, f => f.Hashids.Encode())
                .RuleFor(sd => sd.BinaryLogFile, f => f.System.FileName())
                .Generate();

            var checkRun = new Faker<CheckRun>()
                .RuleFor(c => c.HtmlUrl, f => f.Internet.UrlWithPath())
                .Generate();

            var binaryLogFilePath = new Faker().System.FilePath();

            var binaryLogProcessor = Substitute.For<IBinaryLogProcessor>();
            var tempFileService = Substitute.For<ITempFileService>();
            var checkRunSubmitter = Substitute.For<ICheckRunSubmitter>();

            tempFileService.GetFilePath(null).ReturnsForAnyArgs(binaryLogFilePath);

            var buildDetails = new BuildDetails(new SolutionDetails(submissionData.CloneRoot));
            binaryLogProcessor.ProcessLog(null, null).ReturnsForAnyArgs(buildDetails);

            var checkRunSubmitterFactory = Substitute.For<Func<string, Task<ICheckRunSubmitter>>>();
            checkRunSubmitterFactory.Invoke("").ReturnsForAnyArgs(checkRunSubmitter);

            checkRunSubmitter.SubmitCheckRun(null, null, null, null, null, null, null, DateTimeOffset.MinValue, DateTimeOffset.MinValue).ReturnsForAnyArgs(checkRun);

            var msblocService = new MSBLOCService(binaryLogProcessor, checkRunSubmitterFactory, tempFileService, TestLogger.Create<MSBLOCService>(_testOutputHelper));

            var actualCheckRun = await msblocService.SubmitAsync(submissionData);

            actualCheckRun.Should().BeSameAs(checkRun);

            Received.InOrder(async () =>
            {
                tempFileService.GetFilePath(Arg.Is(submissionData.BinaryLogFile));
                binaryLogProcessor.ProcessLog(Arg.Is(binaryLogFilePath), Arg.Is(submissionData.CloneRoot));
                await checkRunSubmitterFactory.Invoke(Arg.Is(submissionData.RepoOwner));
                await checkRunSubmitter.SubmitCheckRun(
                    buildDetails: Arg.Is(buildDetails),
                    owner: Arg.Is(submissionData.RepoOwner),
                    name: Arg.Is(submissionData.RepoName),
                    headSha: Arg.Is(submissionData.CommitSha),
                    checkRunName: Arg.Any<string>(),
                    checkRunTitle: Arg.Any<string>(),
                    checkRunSummary: Arg.Any<string>(),
                    startedAt: Arg.Any<DateTimeOffset>(),
                    completedAt: Arg.Any<DateTimeOffset>());
            });
        }
    }
}
