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
            var parsedBinaryLog = new ParsedBinaryLog(new BuildWarningEventArgs[0], new BuildErrorEventArgs[0]);
            await AssertSubmitLogs("JustAProgrammer", 
                "TestRepo",
                "2d67ec600fc4ae8549b17c79acea1db1bc1dfad5", 
                "SampleCheckRun",
                parsedBinaryLog,
                "Check Run Title",
                "Check Run Summary",
                new NewCheckRunAnnotation[0]);
        }

        private async Task AssertSubmitLogs(string owner, string name, string headSha, string checkRunName, ParsedBinaryLog parsedBinaryLog, string checkRunTitle, string checkRunSummary, NewCheckRunAnnotation[] expectedAnnotations)
        {
            var checkRunsClient = NSubstitute.Substitute.For<ICheckRunsClient>();
            var submitter = new Submitter(checkRunsClient, TestLogger.Create<Submitter>(_testOutputHelper));

            var checkRun = await submitter.SubmitCheckRun(owner, name, headSha, checkRunName, parsedBinaryLog, checkRunTitle, checkRunSummary);

            Received.InOrder(async () =>
            {
                await checkRunsClient.Received().Create(owner, name, Arg.Any<NewCheckRun>());
            });

            var firstCall = checkRunsClient.ReceivedCalls().First();

            var newCheckRun = (NewCheckRun) firstCall.GetArguments().Last();
            var expectedCheckRun = new NewCheckRun(checkRunName, headSha);
            ShouldBe(newCheckRun, checkRunTitle, checkRunSummary, expectedAnnotations, expectedCheckRun);
        }

        private static void ShouldBe(NewCheckRun newCheckRun, string checkRunTitle, string checkRunSummary,
            NewCheckRunAnnotation[] expectedAnnotations, NewCheckRun expectedCheckRun)
        {
            newCheckRun.Name.Should().Be(expectedCheckRun.Name);
            newCheckRun.HeadSha.Should().Be(expectedCheckRun.HeadSha);
            newCheckRun.Output.Title.Should().Be(checkRunTitle);
            newCheckRun.Output.Summary.Should().Be(checkRunSummary);

            newCheckRun.Output.Annotations.Count.Should().Be(expectedAnnotations.Length);

            for (var index = 0; index < newCheckRun.Output.Annotations.Count; index++)
            {
                var newCheckRunAnnotation = newCheckRun.Output.Annotations[index];
                var expectedAnnotation = expectedAnnotations[index];
            }
        }
    }
}