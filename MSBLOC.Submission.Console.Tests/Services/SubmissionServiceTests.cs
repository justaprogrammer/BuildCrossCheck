using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Text;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MSBLOC.Core.Model.CheckRunSubmission;
using MSBLOC.Core.Model.LogAnalyzer;
using MSBLOC.Core.Tests.Util;
using MSBLOC.Submission.Console.Services;
using Newtonsoft.Json;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace MSBLOC.Submission.Console.Tests.Services
{
    public class SubmissionServiceTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ILogger<SubmissionServiceTests> _logger;

        private static readonly Faker Faker;

        public SubmissionServiceTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _logger = TestLogger.Create<SubmissionServiceTests>(testOutputHelper);
        }

        static SubmissionServiceTests()
        {
            Faker = new Faker();
        }

        [Fact]
        public void ShouldSubmit()
        {
            var mockFileSystem = new MockFileSystem();
            var submissionService = new SubmissionService(mockFileSystem);
            submissionService.Submit(Faker.System.FilePath(), Faker.Random.String());
        }
    }
}