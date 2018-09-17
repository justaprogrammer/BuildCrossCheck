using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MSBLOC.Core.Tests.Util;
using MSBLOC.Submission.Console.Services;
using NSubstitute;
using RestSharp;
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
        public async Task ShouldSubmit()
        {
            var token = Faker.Random.String();
            var inputFile = Faker.System.FilePath();
            var textContents = Faker.Lorem.Paragraph();
            var headSha = Faker.Random.String();

            var mockFileSystem = new MockFileSystem();
            var mockFileData = new MockFileData(textContents);
            mockFileSystem.AddFile(inputFile, mockFileData);

            var restClient = Substitute.For<IRestClient>();

            var submissionService = new SubmissionService(mockFileSystem, restClient);

            await submissionService.Submit(inputFile, token, headSha);

            await restClient.Received(1).ExecutePostTaskAsync(Arg.Any<IRestRequest>());
            var objects = restClient.ReceivedCalls().First().GetArguments();

            var restRequest = (RestRequest)objects[0];
            restRequest.Parameters.Should().BeEquivalentTo(
                    new Parameter
                    {
                        Type = ParameterType.HttpHeader,
                        Name = "Authorization",
                        Value = $"Bearer {token}"
                    },
                    new Parameter
                    {
                        Type = ParameterType.RequestBody,
                        Name = "CommitSha",
                        Value = headSha
                    }
                );

            restRequest.Files.Should().BeEquivalentTo(new FileParameter(){ContentLength = mockFileData.Contents.Length });
        }
    }
}