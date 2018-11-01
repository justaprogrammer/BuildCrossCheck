using System;
using System.Diagnostics.CodeAnalysis;
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
        [SuppressMessage("ReSharper", "ArgumentsStyleOther")]
        [SuppressMessage("ReSharper", "ArgumentsStyleNamedExpression")]
        static CheckRunSubmissionServiceTests()
        {
            Faker = new Faker();
            FakeAnnotation = new Faker<Annotation>()
                .CustomInstantiator(f =>
                {
                    var lineNumber = f.Random.Int(1);
                    return new Annotation(
                        filename: f.System.FileName(),
                        startLine: lineNumber,
                        endLine: lineNumber,
                        checkWarningLevel: f.PickRandom<CheckWarningLevel>(),
                        message: f.Lorem.Word())
                    {
                        Title = f.Random.Words(3)
                    };
                });

            FakeCheckRunImage = new Faker<CheckRunImage>()
                .CustomInstantiator(f => new CheckRunImage(alt: f.Random.Words(3), imageUrl: f.Internet.Url())
                {
                    Caption = f.Random.Words(3)
                });

            FakeCheckRun = new Faker<CreateCheckRun>()
                .CustomInstantiator(f => new CreateCheckRun(
                    name: f.Random.Word(),
                    title: f.Random.Word(),
                    summary: f.Random.Word(),
                    conclusion: f.Random.Enum<CheckConclusion>(),
                    startedAt: f.Date.PastOffset(2),
                    completedAt: f.Date.PastOffset())
                {
                    Annotations = f.Random.Bool() ? null : FakeAnnotation.Generate(f.Random.Int(2, 10)).ToArray(),
                    Images = f.Random.Bool() ? null : FakeCheckRunImage.Generate(f.Random.Int(2, 10)).ToArray()
                });
        }

        public static Faker<CreateCheckRun> FakeCheckRun { get; set; }
        public static Faker<Annotation> FakeAnnotation { get; }
        public static Faker<CheckRunImage> FakeCheckRunImage { get; set; }

        public static Faker Faker { get; }

        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ILogger<GitHubAppModelServiceTests> _logger;

        public CheckRunSubmissionServiceTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _logger = TestLogger.Create<GitHubAppModelServiceTests>(testOutputHelper);
        }

        [Fact]
        public async Task ShouldSubmitCheckRun()
        {
            var createCheckRun = FakeCheckRun.Generate();
            var resourceText = JsonConvert.SerializeObject(createCheckRun);

            var resourcePath = $"{Faker.System.DirectoryPath()}/{Faker.System.FileName(".json")}";
            var mockFileSystem = new MockFileSystem();
            mockFileSystem.AddFile(resourcePath, new MockFileData(resourceText, Encoding.UTF8));

            var gitHubAppModelService = Substitute.For<IGitHubAppModelService>();

            var checkRunSubmissionService = new CheckRunSubmissionService(
                TestLogger.Create<CheckRunSubmissionService>(_testOutputHelper), 
                mockFileSystem,
                gitHubAppModelService);

            var owner = Faker.Person.UserName;
            var repository = Faker.Lorem.Word();
            var sha = Faker.Random.String();

            await checkRunSubmissionService.SubmitAsync(owner, repository, sha, resourcePath);

            await gitHubAppModelService.Received(1)
                .SubmitCheckRunAsync(owner, repository, sha, Arg.Is<CreateCheckRun>(run => run.Equals(createCheckRun)));
        }
    }
}