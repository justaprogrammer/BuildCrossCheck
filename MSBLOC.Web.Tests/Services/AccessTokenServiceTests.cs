using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using MSBLOC.Core.Interfaces;
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

        public AccessTokenServiceTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _logger = TestLogger.Create<AccessTokenServiceTests>(testOutputHelper);

            IdentityModelEventSource.ShowPII = true;
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

            service.Awaiting(async s => await s.ValidateTokenAsync(jwt)).Should().Throw<InvalidOperationException>();
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
            service.Awaiting(async s => await s.CreateTokenAsync(githubRepositoryId)).Should().Throw<ArgumentException>();
        }

        [Fact]
        public async Task GetTokensForUserRepositoriesTest()
        {
            var optionsAccessor = Substitute.For<IOptions<AuthOptions>>();
            var tokenRepository = Substitute.For<IAccessTokenRepository>();
            var contextAccessor = Substitute.For<IHttpContextAccessor>();

            var (gitHubUserClientFactory, gitHubClient, appsClient) = MockGitHubWithApps();

            var installations = new Faker<Installation>()
                .RuleFor(i => i.Id, f => f.IndexGlobal)
                .Generate(3);

            var installationResults = new InstallationsResponse(installations.Count, installations);
            appsClient.GetAllInstallationsForUser().Returns(installationResults);

            var installationsClient = Substitute.For<IGitHubAppsInstallationsClient>();

            var repositoryIds = new List<long>();

            var respositories = new Faker<Repository>()
                .RuleFor(r => r.Id, f =>
                {
                    var id = f.IndexGlobal;
                    repositoryIds.Add(id);
                    return id;
                });
                
            installationsClient.GetAllRepositoriesForUser(Arg.Any<long>()).Returns(callInfo => new RepositoriesResponse(5, respositories.Generate(5)));

            appsClient.Installations.Returns(installationsClient);

            var accessTokens = Enumerable.Empty<AccessToken>();

            tokenRepository.GetAllAsync(Arg.Any<Expression<Func<AccessToken, bool>>>()).Returns(arg =>
            {
                var predicate = arg.ArgAt<Expression<Func<AccessToken, bool>>>(0).Compile();

                accessTokens = Faker.Random.ListItems(repositoryIds, 3).Select(rid => new AccessToken()
                {
                    GitHubRepositoryId = rid
                });

                return accessTokens.Where(predicate);
            });

            var service = new AccessTokenService(optionsAccessor, tokenRepository, gitHubUserClientFactory, contextAccessor);

            var tokens = await service.GetTokensForUserRepositoriesAsync();

            tokens.Select(t => t.GitHubRepositoryId).Should().BeEquivalentTo(accessTokens.Select(t => t.GitHubRepositoryId));

            await installationsClient.Received(3).GetAllRepositoriesForUser(Arg.Any<long>());
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
