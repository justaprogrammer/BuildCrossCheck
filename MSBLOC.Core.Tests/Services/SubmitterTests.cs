using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;
using MSBLOC.Core.Model;
using MSBLOC.Core.Services;
using MSBLOC.Core.Tests.Util;
using NSubstitute;
using NUnit.Framework;
using Octokit;
using Shouldly;

namespace MSBLOC.Core.Tests.Services
{
    [TestFixture]
    public class SubmitterTests
    {
        private static readonly ILogger<SubmitterTests> logger = TestLogger.Create<SubmitterTests>();

        [Test]
        public async Task ShouldSubmitEmptyLog()
        {
            var parsedBinaryLog = new ParsedBinaryLog(new List<BuildWarningEventArgs>(), new List<BuildErrorEventArgs>());
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
            var submitter = new Submitter(checkRunsClient, TestLogger.Create<Submitter>());

            var checkRun = await submitter.SubmitCheckRun(owner, name, headSha, checkRunName, parsedBinaryLog, checkRunTitle, checkRunSummary);

            Received.InOrder(async () =>
            {
                await checkRunsClient.Received().Create(owner, name, Arg.Any<NewCheckRun>());
            });

            var firstCall = checkRunsClient.ReceivedCalls().First();

            var newCheckRun = (NewCheckRun) firstCall.GetArguments().Last();
            var expectedCheckRun = new NewCheckRun(checkRunName, headSha);
            newCheckRun.ShouldBe(checkRunTitle, checkRunSummary, expectedAnnotations, expectedCheckRun);
        }

    }
}