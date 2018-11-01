using System;
using System.IO.Abstractions.TestingHelpers;
using System.Text;
using System.Threading.Tasks;
using BCC.Core.Model.CheckRunSubmission;
using BCC.Web.Interfaces.GitHub;
using BCC.Web.Models.GitHub;
using BCC.Web.Services;
using BCC.Web.Tests.Util;
using Bogus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace BCC.Web.Tests.Services
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
                    return new Annotation(f.System.FileName(), lineNumber,
                        lineNumber, f.PickRandom<CheckWarningLevel>(), f.Lorem.Word());
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
                Conclusion = Faker.Random.Enum<CheckConclusion>(),
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
                .SubmitCheckRunAsync(owner, repository, sha, createCheckRun);
        }
    }
}