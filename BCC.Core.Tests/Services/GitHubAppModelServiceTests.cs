using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BCC.Core.Interfaces.GitHub;
using BCC.Core.Model.CheckRunSubmission;
using BCC.Core.Services.GitHub;
using BCC.Core.Tests.Util;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Octokit;
using Xunit;
using Xunit.Abstractions;
using CheckRun = Octokit.CheckRun;

namespace BCC.Core.Tests.Services
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
                    return new Annotation(f.System.FileName(), f.PickRandom<Model.CheckRunSubmission.CheckWarningLevel>(),
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

        public static IEnumerable<object[]> ShouldSubmitCheckRunData =>
            new List<object[]>
            {
                new object[] { 1, new int[0] },
                new object[] { 51, new[]{1} },
                new object[] { 100, new[]{50} },
                new object[] { 125, new[]{50, 25} },
            };

        [Theory]
        [MemberData(nameof(ShouldSubmitCheckRunData))]
        public async Task ShouldSubmitCheckRun(int annotationCount, int[] updateCounts)
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
                    Faker.Random.Bool() ? (CheckConclusion?)null : Faker.PickRandom<CheckConclusion>(),
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

            var checkRun = await gitHubAppModelService.SubmitCheckRunAsync(
                owner,
                name,
                Faker.Random.String(),
                Faker.Lorem.Word(),
                Faker.Lorem.Sentence(),
                Faker.Lorem.Paragraph(),
                Faker.Random.Bool(),
                FakeAnnotation.Generate(annotationCount).ToArray(),
                Faker.Date.RecentOffset(2),
                Faker.Date.RecentOffset(1));

            await checkRunsClient.Received(1).Create(owner, name, Arg.Any<NewCheckRun>());
            await checkRunsClient.Received(updateCounts.Length).Update(owner, name, Arg.Any<long>(), Arg.Any<CheckRunUpdate>());
            foreach (var updateCount in updateCounts)
            {
                await checkRunsClient.Received(1).Update(owner, name, Arg.Any<long>(), Arg.Is<CheckRunUpdate>(update => update.Output.Annotations.Count == updateCount));
            }

            checkRun.Id.Should().Be(id);
            checkRun.Url.Should().Be(htmlUrl);
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
                    Faker.Random.Bool() ? (CheckConclusion?)null : Faker.PickRandom<CheckConclusion>(),
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
                    Faker.Random.Bool() ? (CheckConclusion?)null : Faker.PickRandom<CheckConclusion>(),
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
                        Faker.Lorem.Sentence(),
                        Faker.Lorem.Paragraph(),
                        annotations1);

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
            checkClient.Run.Returns((ICheckRunsClient)null);

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
            checkClient.Run.Returns((ICheckRunsClient)null);
            var gitHubAppModelService = CreateTarget(checkClient: checkClient);

            gitHubAppModelService.Awaiting(async s =>
                {
                    var annotations1 = FakeAnnotation.Generate(1).ToArray();

                    await s.UpdateCheckRunAsync(
                        Faker.Random.Long(),
                        Faker.Internet.UserName(),
                        Faker.Lorem.Word(),
                        Faker.Lorem.Sentence(),
                        Faker.Lorem.Paragraph(),
                        annotations1);

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
                Faker.Lorem.Sentence(),
                Faker.Lorem.Paragraph(),
                annotations);

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
    }
}