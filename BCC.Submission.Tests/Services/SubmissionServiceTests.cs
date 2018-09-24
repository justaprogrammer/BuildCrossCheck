using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BCC.Core.Tests.Util;
using BCC.Submission.Services;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RestSharp;
using Xunit;
using Xunit.Abstractions;

namespace BCC.Submission.Tests.Services
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

            var restResponse = Substitute.For<IRestResponse>();
            restResponse.StatusCode.Returns(HttpStatusCode.OK);

            var restClient = Substitute.For<IRestClient>();
            restClient.ExecutePostTaskAsync(Arg.Any<IRestRequest>()).Returns(restResponse);

            var submissionService = new SubmissionService(mockFileSystem, restClient);

            var result = await submissionService.SubmitAsync(inputFile, token, headSha);
            result.Should().BeTrue();

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

            var restRequestFile = restRequest.Files[0];
            restRequestFile.Name.Should().Be("LogFile");
            restRequestFile.FileName.Should().Be("file.txt");
            restRequestFile.ContentType.Should().BeNull();
            restRequestFile.ContentLength.Should().Be(mockFileData.Contents.Length);
        }
    }
}