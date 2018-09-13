using System;
using System.IO.Abstractions.TestingHelpers;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using Microsoft.Extensions.Logging;
using MSBLOC.Core.Interfaces;
using MSBLOC.Core.Interfaces.GitHub;
using MSBLOC.Core.Model.CheckRunSubmission;
using MSBLOC.Core.Model.LogAnalyzer;
using MSBLOC.Core.Services;
using MSBLOC.Core.Services.CheckRunSubmission;
using MSBLOC.Core.Tests.Util;
using Newtonsoft.Json;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace MSBLOC.Core.Tests.Services
{
    public class CheckRunSubmissionServiceTests
    {
        static CheckRunSubmissionServiceTests()
        {
            Faker = new Faker();
            FakeAnnotation = new Faker<Annotation>()
                .CustomInstantiator(f =>
                {
                    var lineNumber = f.Random.Int(1);
                    return new Annotation(f.System.FileName(), f.PickRandom<CheckWarningLevel>(),
                        f.Lorem.Word(), f.Lorem.Sentence(), lineNumber, lineNumber);
                });
        }

        public static Faker<Annotation> FakeAnnotation { get; }
        public static Faker Faker { get; }

        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ILogger<GitHubAppModelServiceTests> _logger;

        public CheckRunSubmissionServiceTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _logger = TestLogger.Create<GitHubAppModelServiceTests>(testOutputHelper);
        }

        [Fact]
        public async Task Blah()
        {
            var resourcePath = $"{Faker.System.DirectoryPath()}/{Faker.System.FileName(".json")}";

            var createCheckRun = new CreateCheckRun
            {
                Name = Faker.Lorem.Word(),
                Title = Faker.Lorem.Word(),
                StartedAt = Faker.Date.Past(2),
                CompletedAt = Faker.Date.Past(),
                Success = Faker.Random.Bool(),
                Summary = Faker.Lorem.Paragraph(),
                Annotations = FakeAnnotation.Generate(10).ToArray()
            };

            var resourceText = JsonConvert.SerializeObject(createCheckRun);

            var mockFileSystem = new MockFileSystem();
            mockFileSystem.AddFile(resourcePath, new MockFileData(resourceText, Encoding.UTF8));

            var gitHubAppModelService = Substitute.For<IGitHubAppModelService>();

            var checkRunSubmissionService = new CheckRunSubmissionService(
                TestLogger.Create<CheckRunSubmissionService>(_testOutputHelper), 
                mockFileSystem, gitHubAppModelService);

            var owner = Faker.Person.UserName;
            var repository = Faker.Lorem.Word();
            var sha = Faker.Random.String();

            await checkRunSubmissionService.SubmitAsync(owner, repository, sha, resourcePath);

            await gitHubAppModelService.Received(1)
                .SubmitCheckRun(owner, repository, sha,
                createCheckRun.Name, createCheckRun.Title, createCheckRun.Summary,
                createCheckRun.Success, Arg.Any<Annotation[]>(),
                createCheckRun.StartedAt, createCheckRun.CompletedAt);
        }
    }
}