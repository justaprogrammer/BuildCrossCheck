using System;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using FluentAssertions.Specialized;
using Microsoft.Extensions.Logging;
using MSBLOC.Core.Interfaces;
using MSBLOC.Core.Interfaces.GitHub;
using MSBLOC.Core.Model;
using MSBLOC.Core.Model.LogAnalyzer;
using MSBLOC.Core.Services;
using MSBLOC.Core.Services.GitHub;
using MSBLOC.Core.Tests.Util;
using NSubstitute;
using Octokit;
using Xunit;
using Xunit.Abstractions;
using CheckRun = Octokit.CheckRun;
using CheckWarningLevel = MSBLOC.Core.Model.LogAnalyzer.CheckWarningLevel;

namespace MSBLOC.Core.Tests.Services
{
    public class GitHubAppModelServiceTests
    {
        static GitHubAppModelServiceTests()
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

        public GitHubAppModelServiceTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _logger = TestLogger.Create<GitHubAppModelServiceTests>(testOutputHelper);
        }

        private static readonly Faker Faker;
        private static readonly Faker<Annotation> FakeAnnotation;

        private readonly ILogger<GitHubAppModelServiceTests> _logger;
        private readonly ITestOutputHelper _testOutputHelper;

        private static GitHubAppModelService CreateTarget(
            IGitHubAppClientFactory gitHubAppClientFactory = null,
            IGitHubClient gitHubClient = null,
            IChecksClient checkClient = null,
            IRepositoryContentsClient repositoryContentsClient = null,
            IRepositoriesClient repositoriesClient = null,
            ICheckRunsClient checkRunsClient = null,
            ITokenGenerator tokenGenerator = null)
        {
            if (checkRunsClient == null) checkRunsClient = Substitute.For<ICheckRunsClient>();

            if (checkClient == null)
            {
                checkClient = Substitute.For<IChecksClient>();
                checkClient.Run.Returns(checkRunsClient);
            }

            if (repositoryContentsClient == null) repositoryContentsClient = Substitute.For<IRepositoryContentsClient>();

            if (repositoriesClient == null)
            {
                repositoriesClient = Substitute.For<IRepositoriesClient>();
                repositoriesClient.Content.Returns(repositoryContentsClient);
            }

            if (gitHubClient == null)
            {
                gitHubClient = Substitute.For<IGitHubClient>();
                gitHubClient.Check.Returns(checkClient);
                gitHubClient.Repository.Returns(repositoriesClient);
            }

            if (gitHubAppClientFactory == null)
            {
                gitHubAppClientFactory = Substitute.For<IGitHubAppClientFactory>();
                gitHubAppClientFactory.CreateAppClient(Arg.Any<ITokenGenerator>()).Returns(gitHubClient);
                gitHubAppClientFactory.CreateAppClientForLoginAsync(Arg.Any<ITokenGenerator>(), Arg.Any<string>())
                    .Returns(gitHubClient);
            }


            tokenGenerator = tokenGenerator ?? Substitute.For<ITokenGenerator>();

            return new GitHubAppModelService(gitHubAppClientFactory, tokenGenerator);
        }

        [Fact]
        public async Task ShouldCreateCheckRun()
        {
            var checkRunsClient = Substitute.For<ICheckRunsClient>();

            var id = Faker.Random.Long();
            var htmlUrl = Faker.Internet.Url();

            checkRunsClient.Create(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<NewCheckRun>())
                .Returns(new CheckRun(
                    id,
                    Faker.Random.String(),
                    Faker.Random.String(),
                    Faker.Internet.Url(),
                    htmlUrl,
                    Faker.PickRandom<CheckStatus>(),
                    Faker.Random.Bool() ? (CheckConclusion?) null : Faker.PickRandom<CheckConclusion>(),
                    Faker.Date.RecentOffset(2),
                    Faker.Date.RecentOffset(1),
                    null,
                    Faker.Lorem.Word(),
                    null,
                    null,
                    null));

            var gitHubAppModelService = CreateTarget(checkRunsClient: checkRunsClient);

            var owner = Faker.Internet.UserName();
            var name = Faker.Lorem.Word();

            var checkRun = await gitHubAppModelService.CreateCheckRunAsync(
                owner,
                name,
                Faker.Random.String(),
                Faker.Lorem.Word(),
                Faker.Lorem.Sentence(),
                Faker.Lorem.Paragraph(), 
                Faker.Random.Bool(),
                FakeAnnotation.Generate(1).ToArray(),
                Faker.Date.RecentOffset(2), 
                Faker.Date.RecentOffset(1));

            checkRunsClient.Received(1).Create(owner, name, Arg.Any<NewCheckRun>());

            checkRun.Id.Should().Be(id);
            checkRun.Url.Should().Be(htmlUrl);
        }

        [Fact]
        public async Task ShouldCreateCheckRunWithNullAnnotations()
        {
            var checkRunsClient = Substitute.For<ICheckRunsClient>();

            var id = Faker.Random.Long();
            var htmlUrl = Faker.Internet.Url();

            checkRunsClient.Create(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<NewCheckRun>())
                .Returns(new CheckRun(
                    id,
                    Faker.Random.String(),
                    Faker.Random.String(),
                    Faker.Internet.Url(),
                    htmlUrl,
                    Faker.PickRandom<CheckStatus>(),
                    Faker.Random.Bool() ? (CheckConclusion?) null : Faker.PickRandom<CheckConclusion>(),
                    Faker.Date.RecentOffset(2),
                    Faker.Date.RecentOffset(1),
                    null,
                    Faker.Lorem.Word(),
                    null,
                    null,
                    null));

            var gitHubAppModelService = CreateTarget(checkRunsClient: checkRunsClient);

            var owner = Faker.Internet.UserName();
            var name = Faker.Lorem.Word();

            var checkRun = await gitHubAppModelService.CreateCheckRunAsync(
                owner,
                name,
                Faker.Random.String(),
                Faker.Lorem.Word(),
                Faker.Lorem.Sentence(),
                Faker.Lorem.Paragraph(), 
                Faker.Random.Bool(),
                null,
                Faker.Date.RecentOffset(2), 
                Faker.Date.RecentOffset(1));

            checkRunsClient.Received(1).Create(owner, name, Arg.Any<NewCheckRun>());

            checkRun.Id.Should().Be(id);
            checkRun.Url.Should().Be(htmlUrl);
        }

        [Fact]
        public void ShouldNotCreateCheckRunWithMoreThan50Annotations()
        {
            var gitHubAppModelService = CreateTarget();

            gitHubAppModelService.Awaiting(async s =>
                {
                    var annotations = FakeAnnotation.Generate(51).ToArray();

                    await s.CreateCheckRunAsync(
                        Faker.Internet.UserName(),
                        Faker.Lorem.Word(),
                        Faker.Random.String(),
                        Faker.Lorem.Word(), 
                        Faker.Lorem.Sentence(), 
                        Faker.Lorem.Paragraph(), 
                        false,
                        annotations, 
                        Faker.Date.RecentOffset(2), 
                        Faker.Date.RecentOffset(1));

                }).Should()
                .Throw<GitHubAppModelException>()
                .WithMessage("Error creating CheckRun.")
                .WithInnerException<ArgumentException>()
                .WithMessage("Cannot create more than 50 annotations at a time");
        }

        [Fact]
        public void ShouldNotUpdateCheckRunWithMoreThan50Annotations()
        {
            var gitHubAppModelService = CreateTarget();

            gitHubAppModelService.Awaiting(async s =>
                {
                    var annotations1 = FakeAnnotation.Generate(51).ToArray();

                    await s.UpdateCheckRunAsync(
                        Faker.Random.Long(),
                        Faker.Internet.UserName(),
                        Faker.Lorem.Word(),
                        Faker.Random.String(),
                        Faker.Lorem.Sentence(),
                        Faker.Lorem.Paragraph(),
                        annotations1,
                        Faker.Date.RecentOffset(2),
                        Faker.Date.RecentOffset(1));

                }).Should()
                .Throw<GitHubAppModelException>()
                .WithMessage("Error updating CheckRun.")
                .WithInnerException<ArgumentException>()
                .WithMessage("Cannot create more than 50 annotations at a time");
        }

        [Fact]
        public void ShouldThrowOnCreateCheckRunWithoutCheckRunClient()
        {
            var checkClient = Substitute.For<IChecksClient>();
            checkClient.Run.Returns((ICheckRunsClient) null);

            var gitHubAppModelService = CreateTarget(checkClient: checkClient);

            var exceptionAssertions = gitHubAppModelService.Awaiting(async s =>
            {
                var annotations = FakeAnnotation.Generate(1).ToArray();

                await s.CreateCheckRunAsync(
                    Faker.Internet.UserName(),
                    Faker.Lorem.Word(),
                    Faker.Random.String(),
                    Faker.Lorem.Word(), 
                    Faker.Lorem.Sentence(),
                    Faker.Lorem.Paragraph(),
                    false,
                    annotations,
                    Faker.Date.RecentOffset(2), 
                    Faker.Date.RecentOffset(1));

            }).Should().Throw<GitHubAppModelException>();

            exceptionAssertions
                .WithMessage("Error creating CheckRun.")
                .WithInnerException<InvalidOperationException>()
                .WithMessage("ICheckRunsClient is null");
        }

        [Fact]
        public void ShouldThrowOnUpdateCheckRunWithoutCheckRunClient()
        {
            var checkClient = Substitute.For<IChecksClient>();
            checkClient.Run.Returns((ICheckRunsClient) null);
            var gitHubAppModelService = CreateTarget(checkClient: checkClient);

            gitHubAppModelService.Awaiting(async s =>
                {
                    var annotations1 = FakeAnnotation.Generate(1).ToArray();

                    await s.UpdateCheckRunAsync(
                        Faker.Random.Long(),
                        Faker.Internet.UserName(),
                        Faker.Lorem.Word(),
                        Faker.Random.String(),
                        Faker.Lorem.Sentence(),
                        Faker.Lorem.Paragraph(),
                        annotations1,
                        Faker.Date.RecentOffset(2),
                        Faker.Date.RecentOffset(1));

                }).Should().Throw<GitHubAppModelException>()
                .WithMessage("Error updating CheckRun.")
                .WithInnerException<InvalidOperationException>()
                .WithMessage("ICheckRunsClient is null");
        }

        [Fact]
        public async Task ShouldUpdateCheckRunAsync()
        {
            var checkRunsClient = Substitute.For<ICheckRunsClient>();
            var gitHubAppModelService = CreateTarget(checkRunsClient: checkRunsClient);

            var owner = Faker.Internet.UserName();
            var name = Faker.Lorem.Word();
            var checkRunId = Faker.Random.Long();
            var annotations = FakeAnnotation.Generate(1).ToArray();

            await gitHubAppModelService.UpdateCheckRunAsync(
                checkRunId,
                owner,
                name,
                Faker.Random.String(),
                Faker.Lorem.Sentence(),
                Faker.Lorem.Paragraph(),
                annotations,
                Faker.Date.RecentOffset(2),
                Faker.Date.RecentOffset(1));

            await checkRunsClient.Received(1).Update(owner, name, checkRunId, Arg.Any<CheckRunUpdate>());
        }

        [Fact]
        public async Task ShouldGetFileContents()
        {
            var owner = Faker.Internet.UserName();
            var name = Faker.Lorem.Word();
            var path = Faker.System.FilePath();
            var reference = Faker.Random.String();
            var expectedContent = Faker.Lorem.Paragraph();
            var encodedExpectedContent = Convert.ToBase64String(Encoding.UTF8.GetBytes(expectedContent));

            var repositoryContentsClient = Substitute.For<IRepositoryContentsClient>();
            repositoryContentsClient.GetAllContentsByRef(owner, name, path, reference)
                .Returns(new[]
                {
                    new RepositoryContent(null, null, null, 0, ContentType.File, null, null, null, null, null, encodedExpectedContent, null, null)
                });

            var gitHubAppModelService = CreateTarget(repositoryContentsClient: repositoryContentsClient);
            var content = await gitHubAppModelService.GetRepositoryFileAsync(owner, name, path, reference);
            content.Should().Be(expectedContent);
        }

        [Fact]
        public async Task ShouldGetLogAnalyzerConfiguration()
        {
            var owner = Faker.Internet.UserName();
            var name = Faker.Lorem.Word();

            var path = "msbloc.json";

            var reference = Faker.Random.String();

            var expectedContent = @"{rules:[{code:""Code1"", reportAs: ""warning""}, {code:""Code2"", reportAs: ""error""}]}";

            var encodedExpectedContent = Convert.ToBase64String(Encoding.UTF8.GetBytes(expectedContent));

            var repositoryContentsClient = Substitute.For<IRepositoryContentsClient>();
            repositoryContentsClient.GetAllContentsByRef(owner, name, path, reference)
                .Returns(new[]
                {
                    new RepositoryContent(null, null, null, 0, ContentType.File, null, null, null, null, null, encodedExpectedContent, null, null)
                });

            var gitHubAppModelService = CreateTarget(repositoryContentsClient: repositoryContentsClient);
            var analyzerConfiguration = await gitHubAppModelService.GetLogAnalyzerConfigurationAsync(owner, name, reference);
            analyzerConfiguration.Should().NotBeNull();
            analyzerConfiguration.Rules.Should().BeEquivalentTo(
                new LogAnalyzerRule { Code = "Code1", ReportAs = ReportAs.Warning },
                new LogAnalyzerRule { Code = "Code2", ReportAs = ReportAs.Error }
                );
        }

        [Fact]
        public async Task ShouldNotGetLogAnalyzerConfigurationIfDoesntExist()
        {
            var owner = Faker.Internet.UserName();
            var name = Faker.Lorem.Word();
            var reference = Faker.Random.String();

            var gitHubAppModelService = CreateTarget();
            var content = await gitHubAppModelService.GetLogAnalyzerConfigurationAsync(owner, name, reference);
            content.Should().BeNull();
        }
    }
}