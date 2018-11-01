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
        public static Faker Faker { get; } = new Faker();

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
            var createCheckRun = FakerHelpers.FakeCreateCheckRun.Generate();
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