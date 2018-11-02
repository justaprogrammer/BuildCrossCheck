using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BCC.Infrastructure.Interfaces;
using BCC.Infrastructure.Models;
using BCC.Web.Interfaces.GitHub;
using BCC.Web.Models;
using BCC.Web.Models.GitHub;
using BCC.Web.Services;
using BCC.Web.Tests.Util;
using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using Xunit.Abstractions;

namespace BCC.Web.Tests.Services
{
    public class AccessTokenServiceTests
    {
        public AccessTokenServiceTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _logger = TestLogger.Create<AccessTokenServiceTests>(testOutputHelper);

            IdentityModelEventSource.ShowPII = true;
        }

        static AccessTokenServiceTests()
        {
            FakeUserRepository = new Faker<Repository>()
                .RuleFor(u => u.Id, (f, u) => f.Random.Int(0))
                .RuleFor(u => u.Name, (f, u) => f.Person.UserName)
                .RuleFor(u => u.Owner, (f, u) => f.Person.UserName)
                .RuleFor(u => u.Url, (f, u) => f.Internet.Url());

            FakeUserInstallation = new Faker<Installation>()
                .RuleFor(u => u.Id, (f, u) => f.Random.Int(0))
                .RuleFor(u => u.Repositories, (f, u) => FakeUserRepository.Generate(Faker.Random.Int(1, 5)))
                .RuleFor(u => u.Login, (f, u) => f.Person.UserName);

            FakeAccessToken = new Faker<AccessToken>()
                .RuleFor(token => token.Id, (f, t) => f.Random.Guid())
                .RuleFor(token => token.GitHubRepositoryId, (f, t) => f.Random.Long())
                .RuleFor(token => token.IssuedTo, (f, t) => f.Internet.UserName())
                .RuleFor(token => token.IssuedAt, (f, t) => f.Date.PastOffset());
        }

        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ILogger<AccessTokenServiceTests> _logger;

        private static readonly Faker Faker = new Faker();
        private static readonly Faker<Repository> FakeUserRepository;
        private static readonly Faker<Installation> FakeUserInstallation;
        private static readonly Faker<AccessToken> FakeAccessToken;

        private static (long userId, ClaimsPrincipal user) FakeUserClaim()
        {
            var userId = Faker.Random.Long();

            var user = Substitute.For<ClaimsPrincipal>();
            user.Claims.Returns(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            });

            return (userId, user);
        }

        private static IHttpContextAccessor FakeHttpContextAccessor(ClaimsPrincipal user)
        {
            var contextAccessor = Substitute.For<IHttpContextAccessor>();
            contextAccessor.HttpContext.Returns(new DefaultHttpContext
            {
                User = user
            });
            return contextAccessor;
        }

        private static AccessTokenService CreateTarget(
            IOptions<AuthOptions> optionsAccessor = null,
            IAccessTokenRepository tokenRepository = null,
            IGitHubUserModelService gitHubUserModelService = null,
            IHttpContextAccessor contextAccessor = null)
        {
            if (optionsAccessor == null)
            {
                var options = new AuthOptions {Secret = Faker.Random.AlphaNumeric(32)};
                optionsAccessor = Substitute.For<IOptions<AuthOptions>>();
                optionsAccessor.Value.Returns(options);
            }

            tokenRepository = tokenRepository ?? Substitute.For<IAccessTokenRepository>();

            contextAccessor = contextAccessor ?? Substitute.For<IHttpContextAccessor>();
            gitHubUserModelService = gitHubUserModelService ?? Substitute.For<IGitHubUserModelService>();

            var accessTokenService = new AccessTokenService(optionsAccessor, tokenRepository, gitHubUserModelService,
                contextAccessor);
            return accessTokenService;
        }

        [Fact]
        public async Task CreateValidTokenTest()
        {
            AccessToken accessToken = null;
            var tokenRepository = Substitute.For<IAccessTokenRepository>();
            await tokenRepository.AddAsync(Arg.Do<AccessToken>(t => { accessToken = t; }));

            var userRepository = FakeUserRepository.Generate();
            var (githubUserId, user) = FakeUserClaim();

            var contextAccessor = FakeHttpContextAccessor(user);

            var gitHubUserModelService = Substitute.For<IGitHubUserModelService>();
            gitHubUserModelService.GetRepositoryAsync(Arg.Is(userRepository.Id)).Returns(userRepository);

            var service = CreateTarget(
                tokenRepository: tokenRepository,
                gitHubUserModelService: gitHubUserModelService,
                contextAccessor: contextAccessor);

            var jwt = await service.CreateTokenAsync(userRepository.Id);

            await tokenRepository.Received().AddAsync(Arg.Any<AccessToken>());
            accessToken.Should().NotBeNull();

            var jsonWebToken = await service.ValidateTokenAsync(jwt);

            await tokenRepository.Received().GetAsync(accessToken.Id);

            jsonWebToken.Should().NotBeNull();
            jsonWebToken.GetPayloadValue<long>(JwtRegisteredClaimNames.Sub).Should().Be(githubUserId);
            jsonWebToken.GetPayloadValue<string>(JwtRegisteredClaimNames.Aud).Should().Be(".Api");
            jsonWebToken.GetPayloadValue<string>(JwtRegisteredClaimNames.Jti).Should().Be(accessToken.Id.ToString());
            DateTimeOffset.FromUnixTimeSeconds(jsonWebToken.GetPayloadValue<int>(JwtRegisteredClaimNames.Iat)).Should()
                .BeCloseTo(DateTimeOffset.UtcNow, 2000);
            jsonWebToken.GetPayloadValue<long>("urn:bcc:repositoryId").Should().Be(userRepository.Id);
        }

        [Fact]
        public async Task GetTokensForUserRepositoriesTest()
        {
            var contextAccessor = Substitute.For<IHttpContextAccessor>();

            var repositories = FakeUserRepository.Generate(Faker.Random.Int(1, 5)).ToArray();
            var repositoryIds = repositories.Select(repository => repository.Id).ToArray();

            var tokenRepository = Substitute.For<IAccessTokenRepository>();

            var accessTokens = Faker.PickRandom(repositories, Faker.Random.Int(1, repositories.Length))
                .Select(repository =>
                {
                    var accessToken = FakeAccessToken.Generate();
                    accessToken.GitHubRepositoryId = repository.Id;
                    return accessToken;
                }).ToArray();

            tokenRepository.GetByRepositoryIdsAsync(Arg.Is<List<long>>(list => list.SequenceEqual(repositoryIds)))
                .Returns(accessTokens);

            var gitHubUserModelService = Substitute.For<IGitHubUserModelService>();
            gitHubUserModelService.GetRepositoriesAsync().Returns(repositories);

            var service = CreateTarget(
                tokenRepository: tokenRepository,
                gitHubUserModelService: gitHubUserModelService,
                contextAccessor: contextAccessor);

            var tokens = await service.GetTokensForUserRepositoriesAsync();

            tokens.Should().BeEquivalentTo(accessTokens);

            await tokenRepository.Received()
                .GetByRepositoryIdsAsync(Arg.Is<IEnumerable<long>>(longs => longs.SequenceEqual(repositoryIds)));
        }

        [Fact]
        public async Task InvalidTokenVerficationTest()
        {
            var (_, user) = FakeUserClaim();
            var contextAccessor = FakeHttpContextAccessor(user);

            var userRepository = FakeUserRepository.Generate();
            var gitHubUserModelService = Substitute.For<IGitHubUserModelService>();
            gitHubUserModelService.GetRepositoryAsync(userRepository.Id).Returns(userRepository);

            var service = CreateTarget(
                gitHubUserModelService: gitHubUserModelService,
                contextAccessor: contextAccessor);
            var jwt = await service.CreateTokenAsync(userRepository.Id);

            var jsonWebToken = await service.ValidateTokenAsync(jwt);

            jsonWebToken.Should().NotBeNull();
            jsonWebToken.GetPayloadValue<string>(JwtRegisteredClaimNames.Jti).Should().NotBeNullOrWhiteSpace();
            jsonWebToken.GetPayloadValue<string>(JwtRegisteredClaimNames.Jti).Should().HaveLength(36);

            var modifiedToken = jwt + " ";
            service.Awaiting(async s => await s.ValidateTokenAsync(modifiedToken)).Should()
                .Throw<SecurityTokenException>()
                .WithMessage("IDX10508: Signature validation failed. Signature is improperly formatted.");
        }

        [Fact]
        public void NoAccessToRepositoryTest()
        {
            var githubRepositoryId = Faker.Random.Long();

            var service = CreateTarget();
            service.Awaiting(async s => await s.CreateTokenAsync(githubRepositoryId)).Should()
                .Throw<ArgumentException>()
                .WithMessage("Repository does not exist or no permission to access given repository.");
        }

        [Fact]
        public async Task RevokedTokenVerficationTest()
        {
            var tokenRepository = Substitute.For<IAccessTokenRepository>();
            tokenRepository.GetAsync(Arg.Any<Guid>()).Throws(new InvalidOperationException());

            var (_, user) = FakeUserClaim();
            var contextAccessor = FakeHttpContextAccessor(user);

            var userRepository = FakeUserRepository.Generate();
            var gitHubUserModelService = Substitute.For<IGitHubUserModelService>();
            gitHubUserModelService.GetRepositoryAsync(userRepository.Id).Returns(userRepository);

            var service = CreateTarget(
                tokenRepository: tokenRepository,
                gitHubUserModelService: gitHubUserModelService,
                contextAccessor: contextAccessor);
            var jwt = await service.CreateTokenAsync(userRepository.Id);

            service.Awaiting(async s => await s.ValidateTokenAsync(jwt)).Should()
                .Throw<InvalidOperationException>()
                .WithMessage("Operation is not valid due to the current state of the object.");
        }
    }
}