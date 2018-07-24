using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MSBLOC.Core.Model;
using MSBLOC.Core.Services;
using MSBLOC.Core.Tests.Util;
using NSubstitute;
using Octokit;
using Xunit;
using Xunit.Abstractions;

namespace MSBLOC.Core.Tests.Services
{
    public class SubmitterTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ILogger<SubmitterTests> _logger;

        public SubmitterTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _logger = TestLogger.Create<SubmitterTests>(testOutputHelper);
        }

        [Fact]
        public async Task ShouldSubmitEmptyLog()
        {
            var cloneRoot = "c:\\Project\\";
            var solutionDetails = new SolutionDetails(cloneRoot);

            var project = new ProjectDetails(cloneRoot, @"C:\projects\testconsoleapp1\TestConsoleApp1.sln");
            solutionDetails.Add(project);

            project = new ProjectDetails(cloneRoot, @"c:\Project\TestConsoleApp1\TestConsoleApp1.csproj");
            project.AddItems("File.cs");
            solutionDetails.Add(project);

            var parsedBinaryLog = new BuildDetails(solutionDetails);
            await AssertSubmitLogs(
                cloneRoot: cloneRoot, 
                owner: "JustAProgrammer", 
                name: "TestRepo",
                headSha: "2d67ec600fc4ae8549b17c79acea1db1bc1dfad5",
                checkRunName: "SampleCheckRun",
                buildDetails: parsedBinaryLog, 
                checkRunTitle: "Check Run Title", 
                checkRunSummary: "Check Run Summary", 
                startedAt: DateTimeOffset.Now.Subtract(TimeSpan.FromMinutes(5)), 
                completedAt: DateTimeOffset.Now,
                expectedAnnotations: new NewCheckRunAnnotation[0],
                expectedConclusion: CheckConclusion.Success);
        }

        [Fact]
        public async Task ShouldSubmitLogWithWarning()
        {
            var cloneRoot = "c:\\Project\\";
            var solutionDetails = new SolutionDetails(cloneRoot);

            var project = new ProjectDetails(cloneRoot, @"C:\projects\testconsoleapp1\TestConsoleApp1.sln");
            solutionDetails.Add(project);

            project = new ProjectDetails(cloneRoot, @"c:\Project\TestConsoleApp1\TestConsoleApp1.csproj");
            project.AddItems("File.cs");
            solutionDetails.Add(project);

            var buildDetails = new BuildDetails(
                solutionDetails,
                new[] {
                    new Annotation(@"TestConsoleApp1/File.cs", AnnotationWarningLevel.Warning, "Title", "Message", 9, 9)
                });

            await AssertSubmitLogs(
                cloneRoot: cloneRoot,
                owner: "JustAProgrammer", 
                name: "TestRepo", 
                headSha: "2d67ec600fc4ae8549b17c79acea1db1bc1dfad5",
                checkRunName: "SampleCheckRun",
                buildDetails: buildDetails, 
                checkRunTitle: "Check Run Title", 
                checkRunSummary: "Check Run Summary", 
                startedAt: DateTimeOffset.Now.Subtract(TimeSpan.FromMinutes(5)), 
                completedAt: DateTimeOffset.Now,
                expectedAnnotations: new[]
                {
                    new NewCheckRunAnnotation(@"TestConsoleApp1/File.cs", "https://github.com/JustAProgrammer/TestRepo/blob/2d67ec600fc4ae8549b17c79acea1db1bc1dfad5/TestConsoleApp1/File.cs", 9, 9, CheckWarningLevel.Warning, "Message")
                    {
                        Title = "Title"
                    }
                }, 
                expectedConclusion: CheckConclusion.Success);
        }

        [Fact]
        public async Task ShouldSubmitLogWithError()
        {
            var cloneRoot = "c:\\Project\\";
            var solutionDetails = new SolutionDetails(cloneRoot);

            var project = new ProjectDetails(cloneRoot, @"C:\projects\testconsoleapp1\TestConsoleApp1.sln");
            solutionDetails.Add(project);

            project = new ProjectDetails(cloneRoot, @"c:\Project\TestConsoleApp1\TestConsoleApp1.csproj");
            project.AddItems("File.cs");
            solutionDetails.Add(project);

            var buildDetails = new BuildDetails(
                solutionDetails,
                new[] {
                    new Annotation(@"TestConsoleApp1/File.cs", AnnotationWarningLevel.Failure, "Title", "Message", 9, 9)
                });

            await AssertSubmitLogs(
                cloneRoot: cloneRoot,
                owner: "JustAProgrammer",
                name: "TestRepo",
                headSha: "2d67ec600fc4ae8549b17c79acea1db1bc1dfad5",
                checkRunName: "SampleCheckRun",
                buildDetails: buildDetails,
                checkRunTitle: "Check Run Title",
                checkRunSummary: "Check Run Summary", 
                startedAt: DateTimeOffset.Now.Subtract(TimeSpan.FromMinutes(5)), 
                completedAt: DateTimeOffset.Now,
                expectedAnnotations: new[]
                {
                    new NewCheckRunAnnotation(@"TestConsoleApp1/File.cs", "https://github.com/JustAProgrammer/TestRepo/blob/2d67ec600fc4ae8549b17c79acea1db1bc1dfad5/TestConsoleApp1/File.cs", 9, 9, CheckWarningLevel.Failure, "Message")
                    {
                        Title = "Title"
                    }
                }, 
                expectedConclusion: CheckConclusion.Failure);
        }

        private async Task AssertSubmitLogs(string cloneRoot, string owner, string name, string headSha,
            string checkRunName, BuildDetails buildDetails, string checkRunTitle, string checkRunSummary,
            DateTimeOffset startedAt, DateTimeOffset completedAt, NewCheckRunAnnotation[] expectedAnnotations, 
            CheckConclusion expectedConclusion)
        {
            var checkRunsClient = Substitute.For<ICheckRunsClient>();
            checkRunsClient.Create(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<NewCheckRun>())
                .ReturnsForAnyArgs(new CheckRun());

            var submitter = new Submitter(checkRunsClient, TestLogger.Create<Submitter>(_testOutputHelper));

            await submitter.SubmitCheckRun(owner, name, headSha, checkRunName, buildDetails, checkRunTitle, checkRunSummary, startedAt, completedAt, cloneRoot);

            Received.InOrder(async () =>
            {
                await checkRunsClient.Received().Create(owner, name, Arg.Any<NewCheckRun>());
            });

            var firstCall = checkRunsClient.ReceivedCalls().First();

            var newCheckRun = (NewCheckRun)firstCall.GetArguments().Last();
            var expectedCheckRun = new NewCheckRun(checkRunName, headSha)
            {
                Output = new NewCheckRunOutput(checkRunTitle, checkRunSummary)
                {
                    Annotations = expectedAnnotations
                },
                Status = CheckStatus.Completed,
                StartedAt = startedAt,
                CompletedAt = completedAt,
                Conclusion = expectedConclusion
            };

            newCheckRun.Should().BeEquivalentTo(expectedCheckRun);
        }
    }
}