using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Build.Framework;
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
            var parsedBinaryLog = new ParsedBinaryLog(new BuildWarningEventArgs[0], new BuildErrorEventArgs[0], new Dictionary<string, Dictionary<string, string>>());
            await AssertSubmitLogs(
                cloneRoot: @"c:\Project\", 
                owner: "JustAProgrammer", 
                name: "TestRepo",
                headSha: "2d67ec600fc4ae8549b17c79acea1db1bc1dfad5",
                checkRunName: "SampleCheckRun",
                parsedBinaryLog: parsedBinaryLog, 
                checkRunTitle: "Check Run Title", 
                checkRunSummary: "Check Run Summary", 
                expectedAnnotations: new NewCheckRunAnnotation[0]);
        }

        [Fact]
        public async Task ShouldSubmitLogWithWarning()
        {
            var parsedBinaryLog = new ParsedBinaryLog(
                new[] {
                    new BuildWarningEventArgs(string.Empty, "Code", "File.cs", 9, 0, 0, 0, "Message", string.Empty, string.Empty, DateTime.Now, null)
                    {
                        ProjectFile = @"c:\Project\TestConsoleApp1\TestConsoleApp1.csproj"
                    }
                },
                new BuildErrorEventArgs[0],
                new Dictionary<string, Dictionary<string, string>>
                {
                    {
                        @"c:\Project\TestConsoleApp1\TestConsoleApp1.csproj",
                        new Dictionary<string, string>
                        {
                            {
                                "File.cs", @"c:\Project\TestConsoleApp1\File.cs"
                            }
                        }
                    }
                });

            await AssertSubmitLogs(
                cloneRoot: @"c:\Project\",
                owner: "JustAProgrammer", 
                name: "TestRepo", 
                headSha: "2d67ec600fc4ae8549b17c79acea1db1bc1dfad5",
                checkRunName: "SampleCheckRun",
                parsedBinaryLog: parsedBinaryLog, 
                checkRunTitle: "Check Run Title", 
                checkRunSummary: "Check Run Summary", 
                expectedAnnotations: new[]
                {
                    new NewCheckRunAnnotation(@"TestConsoleApp1\File.cs", "", 9, 9, CheckWarningLevel.Warning, "Message")
                    {
                        Title = "Code"
                    }
                });
        }

        [Fact]
        public async Task ShouldSubmitLogWithError()
        {
            var parsedBinaryLog = new ParsedBinaryLog(
                new BuildWarningEventArgs[0],
                new[] {
                    new BuildErrorEventArgs(string.Empty, "Code", "File.cs", 9, 0, 0, 0, "Message", string.Empty, string.Empty, DateTime.Now, null)
                    {
                        ProjectFile = @"c:\Project\TestConsoleApp1\TestConsoleApp1.csproj"
                    }
                },
                new Dictionary<string, Dictionary<string, string>>
                {
                    {
                        @"c:\Project\TestConsoleApp1\TestConsoleApp1.csproj",
                        new Dictionary<string, string>
                        {
                            {
                                "File.cs", @"c:\Project\TestConsoleApp1\File.cs"
                            }
                        }
                    }
                });

            await AssertSubmitLogs(
                cloneRoot: "c:\\Project\\",
                owner: "JustAProgrammer",
                name: "TestRepo",
                headSha: "2d67ec600fc4ae8549b17c79acea1db1bc1dfad5",
                checkRunName: "SampleCheckRun",
                parsedBinaryLog: parsedBinaryLog,
                checkRunTitle: "Check Run Title",
                checkRunSummary: "Check Run Summary",
                expectedAnnotations: new[]
                {
                    new NewCheckRunAnnotation(@"TestConsoleApp1\File.cs", "", 9, 9, CheckWarningLevel.Failure, "Message")
                    {
                        Title = "Code"
                    }
                });
        }

        private async Task AssertSubmitLogs(string cloneRoot, string owner, string name, string headSha,
            string checkRunName, ParsedBinaryLog parsedBinaryLog, string checkRunTitle, string checkRunSummary,
            NewCheckRunAnnotation[] expectedAnnotations)
        {
            var checkRunsClient = Substitute.For<ICheckRunsClient>();
            checkRunsClient.Create(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<NewCheckRun>())
                .ReturnsForAnyArgs(new CheckRun());

            var submitter = new Submitter(checkRunsClient, TestLogger.Create<Submitter>(_testOutputHelper));

            await submitter.SubmitCheckRun(owner, name, headSha, checkRunName, parsedBinaryLog, checkRunTitle, checkRunSummary, cloneRoot);

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
                }
            };

            newCheckRun.Should().BeEquivalentTo(expectedCheckRun);
        }
    }
}