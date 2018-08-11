using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using MSBLOC.Infrastructure.Interfaces;
using MSBLOC.Web.Interfaces;
using MSBLOC.Web.Models;
using MSBLOC.Web.Services;
using MSBLOC.Web.Tests.Util;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Octokit;
using Xunit;
using Xunit.Abstractions;
using AccessToken = MSBLOC.Infrastructure.Models.AccessToken;

namespace MSBLOC.Web.Tests.Services
{
    public class AccessTokenServiceTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ILogger<AccessTokenServiceTests> _logger;

        private static readonly Faker Faker = new Faker();
        private static readonly Faker<Repository> FakeRepository;
        private static readonly Faker<AccessToken> FakeAccessToken;
        private static readonly Faker<Installation> FakeInstallation;

        public AccessTokenServiceTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _logger = TestLogger.Create<AccessTokenServiceTests>(testOutputHelper);

            IdentityModelEventSource.ShowPII = true;
        }

        static AccessTokenServiceTests()
        {
            FakeRepository = new Faker<Repository>()
                .RuleFor(r => r.Id, (f, r) => f.Random.Long());

            FakeAccessToken = new Faker<AccessToken>()
                .RuleFor(token => token.Id, (f, t) => f.Random.Guid())
                .RuleFor(token => token.GitHubRepositoryId, (f, t) => f.Random.Long())
                .RuleFor(token => token.IssuedTo, (f, t) => f.Internet.UserName())
                .RuleFor(token => token.IssuedAt, (f, t) => f.Date.PastOffset());
            FakeInstallation = new Faker<Installation>()
                .RuleFor(i => i.Id, f => f.IndexGlobal);
        }

        [Fact]
        public async Task CreateValidTokenTest()
        {
            var options = new AuthOptions {Secret = new Faker().Random.AlphaNumeric(32)};
            var optionsAccessor = Substitute.For<IOptions<AuthOptions>>();
            optionsAccessor.Value.Returns(options);

            AccessToken accessToken = null;
            var tokenRepository = Substitute.For<IAccessTokenRepository>();
            await tokenRepository.AddAsync(Arg.Do<AccessToken>(t => { accessToken = t; }));

            var githubRepositoryId = Faker.Random.Long();
            var githubUserId = Faker.Random.Long();

            var user = Substitute.For<ClaimsPrincipal>();
            user.Claims.Returns(new []
            {
                new Claim(ClaimTypes.NameIdentifier, githubUserId.ToString())
            });

            var (gitHubUserClientFactory, gitHubClient, repositoriesClient) = MockGitHubWithRespository();
            repositoriesClient.Get(Arg.Is(githubRepositoryId)).Returns(Task.FromResult(new Repository(githubRepositoryId)));

            var contextAccessor = Substitute.For<IHttpContextAccessor>();
            contextAccessor.HttpContext.Returns(new DefaultHttpContext
            {
                User = user
            });

            var service = new AccessTokenService(optionsAccessor, tokenRepository, gitHubUserClientFactory, contextAccessor);

            var jwt = await service.CreateTokenAsync(githubRepositoryId);

            await tokenRepository.Received().AddAsync(Arg.Any<AccessToken>());
            accessToken.Should().NotBeNull();

            var jsonWebToken = await service.ValidateTokenAsync(jwt);

            await tokenRepository.Received().GetAsync(accessToken.Id);

            jsonWebToken.Should().NotBeNull();
            jsonWebToken.Payload.Value<long>(JwtRegisteredClaimNames.Sub).Should().Be(githubUserId);
            jsonWebToken.Payload.Value<string>(JwtRegisteredClaimNames.Aud).Should().Be("MSBLOC.Api");
            jsonWebToken.Payload.Value<string>(JwtRegisteredClaimNames.Jti).Should().Be(accessToken.Id.ToString());
            DateTimeOffset.FromUnixTimeSeconds(jsonWebToken.Payload.Value<int>(JwtRegisteredClaimNames.Iat)).Should().BeCloseTo(DateTimeOffset.UtcNow, 1000);
            jsonWebToken.Payload.Value<long>("urn:msbloc:repositoryId").Should().Be(githubRepositoryId);
        }

        [Fact]
        public async Task InvalidTokenVerficationTest()
        {
            var options = new AuthOptions { Secret = new Faker().Random.AlphaNumeric(32) };
            var optionsAccessor = Substitute.For<IOptions<AuthOptions>>();
            optionsAccessor.Value.Returns(options);

            var tokenRepository = Substitute.For<IAccessTokenRepository>();

            var githubRepositoryId = Faker.Random.Long();
            var githubUserId = Faker.Random.Long();

            var user = Substitute.For<ClaimsPrincipal>();
            user.Claims.Returns(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, githubUserId.ToString())
            });

            var (gitHubUserClientFactory, gitHubClient, repositoriesClient) = MockGitHubWithRespository();
            repositoriesClient.Get(Arg.Is(githubRepositoryId)).Returns(Task.FromResult(new Repository(githubRepositoryId)));

            var contextAccessor = Substitute.For<IHttpContextAccessor>();
            contextAccessor.HttpContext.Returns(new DefaultHttpContext
            {
                User = user
            });

            var service = new AccessTokenService(optionsAccessor, tokenRepository, gitHubUserClientFactory, contextAccessor);
            var jwt = await service.CreateTokenAsync(githubRepositoryId);

            var jsonWebToken = await service.ValidateTokenAsync(jwt);

            jsonWebToken.Should().NotBeNull();
            jsonWebToken.Payload.Value<string>(JwtRegisteredClaimNames.Jti).Should().NotBeNullOrWhiteSpace();
            jsonWebToken.Payload.Value<string>(JwtRegisteredClaimNames.Jti).Should().HaveLength(36);

            var modifiedToken = jwt + " ";
            service.Awaiting(async s => await s.ValidateTokenAsync(modifiedToken)).Should().Throw<SecurityTokenException>();
        }

        [Fact]
        public async Task RevokedTokenVerficationTest()
        {
            var options = new AuthOptions { Secret = new Faker().Random.AlphaNumeric(32) };
            var optionsAccessor = Substitute.For<IOptions<AuthOptions>>();
            optionsAccessor.Value.Returns(options);

            var tokenRepository = Substitute.For<IAccessTokenRepository>();
            tokenRepository.GetAsync(Arg.Any<Guid>()).Throws(new InvalidOperationException());

            var githubRepositoryId = Faker.Random.Long();
            var githubUserId = Faker.Random.Long();

            var user = Substitute.For<ClaimsPrincipal>();
            user.Claims.Returns(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, githubUserId.ToString())
            });

            var (gitHubUserClientFactory, gitHubClient, repositoriesClient) = MockGitHubWithRespository();
            repositoriesClient.Get(Arg.Is(githubRepositoryId)).Returns(Task.FromResult(new Repository(githubRepositoryId)));

            var contextAccessor = Substitute.For<IHttpContextAccessor>();
            contextAccessor.HttpContext.Returns(new DefaultHttpContext
            {
                User = user
            });

            var service = new AccessTokenService(optionsAccessor, tokenRepository, gitHubUserClientFactory, contextAccessor);
            var jwt = await service.CreateTokenAsync(githubRepositoryId);

            service.Awaiting(async s => await s.ValidateTokenAsync(jwt))
                .Should().Throw<InvalidOperationException>()
                .WithMessage("Operation is not valid due to the current state of the object.");
        }

        [Fact]
        public void NoAccessToRepositoryTest()
        {
            var optionsAccessor = Substitute.For<IOptions<AuthOptions>>();
            var tokenRepository = Substitute.For<IAccessTokenRepository>();
            var contextAccessor = Substitute.For<IHttpContextAccessor>();

            var githubRepositoryId = Faker.Random.Long();

            var (gitHubUserClientFactory, gitHubClient, repositoriesClient) = MockGitHubWithRespository();
            repositoriesClient.Get(Arg.Is(githubRepositoryId)).Returns(Task.FromResult<Repository>(null));

            var service = new AccessTokenService(optionsAccessor, tokenRepository, gitHubUserClientFactory, contextAccessor);
            service.Awaiting(async s => await s.CreateTokenAsync(githubRepositoryId))
                .Should().Throw<ArgumentException>()
                .WithMessage("Repository does not exist or no permission to access given repository.");
        }

        [Fact]
        public async Task GetTokensForUserRepositoriesTest()
        {
            var optionsAccessor = Substitute.For<IOptions<AuthOptions>>();
            var tokenRepository = Substitute.For<IAccessTokenRepository>();
            var contextAccessor = Substitute.For<IHttpContextAccessor>();

            var (gitHubUserClientFactory, gitHubClient, appsClient) = MockGitHubWithApps();

            var installation0 = FakeInstallation.Generate();
            var repositories0 = FakeRepository.Generate(Faker.Random.Int(2, 3));

            var installation1 = FakeInstallation.Generate();
            var repositories1 = FakeRepository.Generate(Faker.Random.Int(2, 3));

            var installations = new[] { installation0, installation1 };

            var installationResults = new InstallationsResponse(installations.Length, installations);
            appsClient.GetAllInstallationsForUser().Returns(installationResults);

            var installationsClient = Substitute.For<IGitHubAppsInstallationsClient>();

            var repositories = repositories0.Union(repositories1).ToArray();
            var repositoryIds = repositories.Select(repository => repository.Id).ToArray();

            installationsClient.GetAllRepositoriesForUser(installation0.Id).Returns(new RepositoriesResponse(repositories0.Count, repositories0));
            installationsClient.GetAllRepositoriesForUser(installation1.Id).Returns(new RepositoriesResponse(repositories1.Count, repositories1));

            appsClient.Installations.Returns(installationsClient);

            var accessTokens = Faker.PickRandom(repositories, Faker.Random.Int(1, repositories.Length))
                .Select(repository =>
                {
                    var accessToken = FakeAccessToken.Generate();
                    accessToken.GitHubRepositoryId = repository.Id;
                    return accessToken;
                }).ToArray();

            tokenRepository.GetByRepositoryIds(Arg.Is<List<long>>(r => r.SequenceEqual(repositoryIds))).Returns(accessTokens);

            var service = new AccessTokenService(optionsAccessor, tokenRepository, gitHubUserClientFactory, contextAccessor);

            var tokens = await service.GetTokensForUserRepositoriesAsync();

            tokens.Should().BeEquivalentTo(accessTokens);

            Received.InOrder(async () =>
            {
                foreach (var installation in installations)
                {
                    await installationsClient.Received().GetAllRepositoriesForUser(installation.Id);
                }

                await tokenRepository.Received().GetByRepositoryIds(Arg.Is<IEnumerable<long>>(longs => longs.SequenceEqual(repositoryIds)));
            });
        }

        private (IGitHubUserClientFactory gitHubUserClientFactory, IGitHubClient gitHubClient) MockGitHub()
        {
            var gitHubClient = Substitute.For<IGitHubClient>();

            var gitHubUserClientFactory = Substitute.For<IGitHubUserClientFactory>();
            gitHubUserClientFactory.CreateClient().Returns(Task.FromResult(gitHubClient));

            return (gitHubUserClientFactory, gitHubClient);
        }

        private (IGitHubUserClientFactory gitHubUserClientFactory, IGitHubClient gitHubClient, IRepositoriesClient repositoriesClient) MockGitHubWithRespository()
        {
            var (gitHubUserClientFactory, gitHubClient) = MockGitHub();

            var repositoriesClient = Substitute.For<IRepositoriesClient>();

            gitHubClient.Repository.Returns(repositoriesClient);

            return (gitHubUserClientFactory, gitHubClient, repositoriesClient);
        }

        private (IGitHubUserClientFactory gitHubUserClientFactory, IGitHubClient gitHubClient, IGitHubAppsClient appsClient) MockGitHubWithApps()
        {
            var (gitHubUserClientFactory, gitHubClient) = MockGitHub();

            var appsClient = Substitute.For<IGitHubAppsClient>();

            gitHubClient.GitHubApps.Returns(appsClient);

            return (gitHubUserClientFactory, gitHubClient, appsClient);
        }
    }
}
