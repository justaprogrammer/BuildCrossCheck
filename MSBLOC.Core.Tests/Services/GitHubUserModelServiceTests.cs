using System;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MSBLOC.Core.Interfaces;
using MSBLOC.Core.Interfaces.GitHub;
using MSBLOC.Core.Services;
using MSBLOC.Core.Services.GitHub;
using MSBLOC.Core.Tests.Util;
using NSubstitute;
using Octokit;
using Xunit;
using Xunit.Abstractions;

namespace MSBLOC.Core.Tests.Services
{
    public class GitHubUserModelServiceTests
    {
        public GitHubUserModelServiceTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _logger = TestLogger.Create<GitHubUserModelServiceTests>(testOutputHelper);
        }

        static GitHubUserModelServiceTests()
        {
            var appId = Faker.Random.Long(0);

            var fakeUser = new Faker<User>()
                .CustomInstantiator(f =>
                {
                    string avatarUrl = f.Internet.Url();
                    string bio = f.Lorem.Paragraph(1);
                    string blog = f.Internet.Url();
                    int collaborators = f.Random.Int(0);
                    string company = f.Random.Bool() ? null : f.Company.CompanyName();
                    DateTimeOffset createdAt = f.Date.PastOffset(2);
                    DateTimeOffset updatedAt = f.Date.PastOffset(1);
                    int diskUsage = f.Random.Int(0);
                    string email = f.Internet.Email();
                    int followers = f.Random.Int(0);
                    int following = f.Random.Int(0);
                    bool? hireable = f.Random.Bool() ? (bool?) null : f.Random.Bool();
                    string htmlUrl = f.Internet.Url();
                    int totalPrivateRepos = f.Random.Int(0);
                    int id = f.Random.Int(0);
                    string location = f.Address.City();
                    string login = f.Internet.UserName();
                    string name = f.Person.FullName;
                    string nodeId = f.Random.String();
                    int ownedPrivateRepos = f.Random.Int(0);
                    Plan plan = null;
                    int privateGists = f.Random.Int(0);
                    int publicGists = f.Random.Int(0);
                    int publicRepos = f.Random.Int(0);
                    string url = f.Internet.Url();
                    RepositoryPermissions permissions =
                        new RepositoryPermissions(f.Random.Bool(), f.Random.Bool(), f.Random.Bool());
                    bool siteAdmin = f.Random.Bool();
                    string ldapDistinguishedName = f.Person.FullName;
                    DateTimeOffset? suspendedAt = null;

                    return new User(avatarUrl, bio, blog, collaborators, company, createdAt, updatedAt, diskUsage,
                        email, followers, following, hireable, htmlUrl, totalPrivateRepos, id, location, login,
                        name, nodeId, ownedPrivateRepos, plan, privateGists, publicGists, publicRepos, url,
                        permissions, siteAdmin, ldapDistinguishedName, suspendedAt);
                });

            FakeInstallation = new Faker<Installation>()
                .RuleFor(i => i.Id, (f, i) => f.Random.Long(0))
                .RuleFor(i => i.AppId, (f, i) => appId)
                .RuleFor(i => i.Account, (f, i) => fakeUser.Generate());

            var fakeRepository = new Faker<Repository>()
                .RuleFor(response => response.Id, (faker1, repository) => faker1.Random.Long(0))
                .RuleFor(response => response.Owner, (faker1, repository) => fakeUser.Generate())
                .RuleFor(response => response.Name, (faker1, repository) => faker1.Lorem.Word())
                .RuleFor(response => response.Url, (faker1, repository) => faker1.Internet.Url());

            FakeRepositoriesResponse = new Faker<RepositoriesResponse>()
                .RuleFor(r => r.Repositories, (f, r) => fakeRepository.Generate(Faker.Random.Int(1, 10)))
                .RuleFor(r => r.TotalCount, (f, r) => r.Repositories.Count);
        }

        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ILogger<GitHubUserModelServiceTests> _logger;

        private static readonly Faker Faker = new Faker();
        private static readonly Faker<RepositoriesResponse> FakeRepositoriesResponse;
        private static readonly Faker<Installation> FakeInstallation;

        private static GitHubUserModelService CreateTarget(
            IGitHubAppInstallationsClient gitHubAppsInstallationsClient = null,
            IGitHubAppsClient gitHubAppsClient = null,
            IGitHubUserClientFactory gitHubUserClientFactory = null,
            IGitHubClient gitHubClient = null)
        {
            gitHubAppsInstallationsClient =
                gitHubAppsInstallationsClient ?? Substitute.For<IGitHubAppInstallationsClient>();

            gitHubAppsClient = gitHubAppsClient ?? Substitute.For<IGitHubAppsClient>();
            gitHubAppsClient.Installation.Returns(gitHubAppsInstallationsClient);

            gitHubClient = gitHubClient ?? Substitute.For<IGitHubClient>();
            gitHubClient.GitHubApps.Returns(gitHubAppsClient);

            gitHubUserClientFactory = gitHubUserClientFactory ?? Substitute.For<IGitHubUserClientFactory>();
            gitHubUserClientFactory.CreateClient().Returns(gitHubClient);

            var gitHubUserModelService = new GitHubUserModelService(gitHubUserClientFactory);
            return gitHubUserModelService;
        }

        [Fact]
        public async Task ShouldGetUserInstallations()
        {
            var installation1 = FakeInstallation.Generate();
            var repositoriesResponse1 = FakeRepositoriesResponse.Generate();

            var installation2 = FakeInstallation.Generate();
            var repositoriesResponse2 = FakeRepositoriesResponse.Generate();

            var installationsResponse = new InstallationsResponse(2, new[] {installation1, installation2});

            var gitHubAppsClient = Substitute.For<IGitHubAppsClient>();
            gitHubAppsClient.GetAllInstallationsForCurrentUser()
                .Returns(installationsResponse);

            var gitHubAppsInstallationsClient = Substitute.For<IGitHubAppInstallationsClient>();

            gitHubAppsInstallationsClient.GetAllRepositoriesForCurrentUser(installation1.Id)
                .Returns(repositoriesResponse1);

            gitHubAppsInstallationsClient.GetAllRepositoriesForCurrentUser(installation2.Id)
                .Returns(repositoriesResponse2);

            var gitHubUserModelService = CreateTarget(
                gitHubAppsClient: gitHubAppsClient,
                gitHubAppsInstallationsClient: gitHubAppsInstallationsClient
            );

            var userInstallations = await gitHubUserModelService.GetInstallationsAsync();
            userInstallations.Count.Should().Be(2);

            userInstallations[0].Id.Should().Be(installation1.Id);
            userInstallations[0].Repositories.Count.Should().Be(repositoriesResponse1.Repositories.Count);
            userInstallations[0].Repositories[0].Id.Should().Be(repositoriesResponse1.Repositories[0].Id);

            userInstallations[1].Id.Should().Be(installation2.Id);
            userInstallations[1].Repositories.Count.Should().Be(repositoriesResponse2.Repositories.Count);
            userInstallations[1].Repositories[0].Id.Should().Be(repositoriesResponse2.Repositories[0].Id);
        }

        [Fact]
        public async Task ShouldGetUserRepositories()
        {
            var installation1 = FakeInstallation.Generate();
            var repositoriesResponse1 = FakeRepositoriesResponse.Generate();

            var installation2 = FakeInstallation.Generate();
            var repositoriesResponse2 = FakeRepositoriesResponse.Generate();

            var installationsResponse = new InstallationsResponse(2, new[] {installation1, installation2});

            var gitHubAppsClient = Substitute.For<IGitHubAppsClient>();
            gitHubAppsClient.GetAllInstallationsForCurrentUser()
                .Returns(installationsResponse);

            var gitHubAppsInstallationsClient = Substitute.For<IGitHubAppInstallationsClient>();

            gitHubAppsInstallationsClient.GetAllRepositoriesForCurrentUser(installation1.Id)
                .Returns(repositoriesResponse1);

            gitHubAppsInstallationsClient.GetAllRepositoriesForCurrentUser(installation2.Id)
                .Returns(repositoriesResponse2);

            var gitHubUserModelService = CreateTarget(
                gitHubAppsClient: gitHubAppsClient,
                gitHubAppsInstallationsClient: gitHubAppsInstallationsClient
            );

            var repositories = await gitHubUserModelService.GetRepositoriesAsync();

            repositories.Count.Should().Be(repositoriesResponse1.TotalCount + repositoriesResponse2.TotalCount);
        }
    }
}