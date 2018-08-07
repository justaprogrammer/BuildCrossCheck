using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MoreLinq;
using MSBLOC.Core.Model;
using MSBLOC.Core.Services;
using MSBLOC.Core.Tests.Util;
using NSubstitute;
using Octokit;
using Xunit;
using Xunit.Abstractions;

namespace MSBLOC.Core.Tests.Services
{
    public class CheckRunSubmitterTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ILogger<CheckRunSubmitterTests> _logger;

        public CheckRunSubmitterTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _logger = TestLogger.Create<CheckRunSubmitterTests>(testOutputHelper);
        }

        [Fact]
        public async Task ShouldSubmitEmptyLog()
        {
            var cloneRoot = "c:\\Projects\\";
            var solutionDetails = new SolutionDetails(cloneRoot);

            var project = new ProjectDetails(cloneRoot, @"C:\projects\testconsoleapp1\TestConsoleApp1.sln");
            solutionDetails.Add(project);

            project = new ProjectDetails(cloneRoot, @"c:\Projects\TestConsoleApp1\TestConsoleApp1.csproj");
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
                expectedAnnotations: null,
                expectedConclusion: CheckConclusion.Success);
        }

        [Fact]
        public async Task ShouldSubmitLogWithWarning()
        {
            var cloneRoot = "c:\\Projects\\";
            var solutionDetails = new SolutionDetails(cloneRoot);

            var project = new ProjectDetails(cloneRoot, @"C:\projects\testconsoleapp1\TestConsoleApp1.sln");
            solutionDetails.Add(project);

            project = new ProjectDetails(cloneRoot, @"c:\Projects\TestConsoleApp1\TestConsoleApp1.csproj");
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
            var cloneRoot = "c:\\Projects\\";
            var solutionDetails = new SolutionDetails(cloneRoot);

            var project = new ProjectDetails(cloneRoot, @"C:\projects\testconsoleapp1\TestConsoleApp1.sln");
            solutionDetails.Add(project);

            project = new ProjectDetails(cloneRoot, @"c:\Projects\TestConsoleApp1\TestConsoleApp1.csproj");
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


        [Theory]
        [InlineData(49, 0)]
        [InlineData(50, 0)]
        [InlineData(51, 1)]
        [InlineData(201, 4)]
        public async Task ShouldSubmitLogWithMultipleAnnotations(int count, int expectedUpdateCount)
        {
            var cloneRoot = "c:\\Projects\\";
            var solutionDetails = new SolutionDetails(cloneRoot);

            var project = new ProjectDetails(cloneRoot, @"C:\projects\testconsoleapp1\TestConsoleApp1.sln");
            solutionDetails.Add(project);

            project = new ProjectDetails(cloneRoot, @"c:\Projects\TestConsoleApp1\TestConsoleApp1.csproj");
            project.AddItems("File.cs");
            solutionDetails.Add(project);

            var annotations =
                Enumerable.Range(0, count)
                    .Select(i => new Annotation($@"TestConsoleApp1/File-{i}.cs", AnnotationWarningLevel.Warning, $"Title-{i}", $"Message-{i}", i, i));

            var buildDetails = new BuildDetails(
                solutionDetails,
                annotations);


            var newCheckRunAnnotations = 
                Enumerable.Range(0, count)
                    .Select(i =>
                    {
                        var newCheckRunAnnotation = new NewCheckRunAnnotation(
                            filename: $@"TestConsoleApp1/File-{i}.cs",
                            blobHref: $"https://github.com/JustAProgrammer/TestRepo/blob/2d67ec600fc4ae8549b17c79acea1db1bc1dfad5/TestConsoleApp1/File-{i}.cs",
                            startLine: i, 
                            endLine: i, 
                            warningLevel: CheckWarningLevel.Warning,
                            message: $"Message-{i}")
                        {
                            Title = $"Title-{i}"
                        };

                        return newCheckRunAnnotation;
                    }).ToArray();

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
                expectedAnnotations: newCheckRunAnnotations,
                expectedConclusion: CheckConclusion.Success,
                expectedUpdateCalls: expectedUpdateCount);
        }

        private async Task AssertSubmitLogs(string cloneRoot, string owner, string name, string headSha,
            string checkRunName, BuildDetails buildDetails, string checkRunTitle, string checkRunSummary,
            DateTimeOffset startedAt, DateTimeOffset completedAt, NewCheckRunAnnotation[] expectedAnnotations, 
            CheckConclusion expectedConclusion, int expectedUpdateCalls = 0)
        {
            var random = new Random();
            var expectedCheckRunId = (long)random.NextDouble();

            var checkRunsClient = Substitute.For<ICheckRunsClient>();
            checkRunsClient.Create(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<NewCheckRun>())
                .ReturnsForAnyArgs(new CheckRun(
                    id: expectedCheckRunId, 
                    headSha: default(string), 
                    externalId: default(string), 
                    url: default(string), 
                    htmlUrl: default(string), 
                    status: default(CheckStatus), 
                    conclusion: default(CheckConclusion?), 
                    startedAt: default(DateTimeOffset), 
                    completedAt: default(DateTimeOffset), 
                    output: default(CheckRunOutputResponse), 
                    name: default(string), 
                    checkSuite: default(CheckSuite), 
                    app: default(GitHubApp), 
                    pullRequests: default(IReadOnlyList<PullRequest>)));

            var submitter = new CheckRunSubmitter(checkRunsClient, TestLogger.Create<CheckRunSubmitter>(_testOutputHelper));

            await submitter.SubmitCheckRun(buildDetails, owner, name, headSha, checkRunName, checkRunTitle, checkRunSummary, startedAt, completedAt);

            var batches = expectedAnnotations?.Batch(50).ToArray();
            var firstBatch = batches?.FirstOrDefault()?.ToArray();
            var remainingAnnotations = batches?.Skip(1).ToArray() ?? new IEnumerable<NewCheckRunAnnotation>[0];

            Received.InOrder(async () =>
            {
                await checkRunsClient.Received().Create(owner, name, Arg.Any<NewCheckRun>());

                for (int i = 0; i < remainingAnnotations.Length; i++)
                {
                    await checkRunsClient.Received().Update(owner, name, Arg.Any<long>(), Arg.Any<CheckRunUpdate>());
                }
            });

            var receivedCalls = checkRunsClient.ReceivedCalls().ToArray();

            receivedCalls.Length.Should().Be(1 + expectedUpdateCalls);

            var firstCall = receivedCalls.First();

            var newCheckRun = (NewCheckRun)firstCall.GetArguments().Last();
            var expectedCheckRun = new NewCheckRun(checkRunName, headSha)
            {
                Output = new NewCheckRunOutput(checkRunTitle, checkRunSummary)
                {
                    Annotations = firstBatch
                },
                Status = CheckStatus.Completed,
                StartedAt = startedAt,
                CompletedAt = completedAt,
                Conclusion = expectedConclusion
            };

            newCheckRun.Should().BeEquivalentTo(expectedCheckRun);

            var remainingCalls = receivedCalls.Skip(1).ToArray();
            for (var index = 0; index < remainingCalls.Length; index++)
            {
                var remainingCall = remainingCalls[index];
                var remainingAnnotation = remainingAnnotations[index].ToArray();
                var arguments = remainingCall.GetArguments();

                long checkRunId = (long) arguments.Skip(2).First();
                checkRunId.Should().Be(expectedCheckRunId);

                var checkRunUpdate = (CheckRunUpdate) arguments.Last();
                var expectedCheckRunUpdate = new CheckRunUpdate
                {
                    Output = new NewCheckRunOutput(checkRunTitle, checkRunSummary)
                    {
                        Annotations = remainingAnnotation
                    },
                    Status = CheckStatus.Completed,
                    StartedAt = startedAt,
                    CompletedAt = completedAt,
                    Conclusion = expectedConclusion
                };

                checkRunUpdate.Should().BeEquivalentTo(expectedCheckRunUpdate);
            }
        }
    }
}