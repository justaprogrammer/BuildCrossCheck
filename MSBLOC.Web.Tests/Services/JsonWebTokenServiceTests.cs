using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using MSBLOC.Infrastructure.Interfaces;
using MSBLOC.Infrastructure.Models;
using MSBLOC.Web.Models;
using MSBLOC.Web.Services;
using MSBLOC.Web.Tests.Util;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using Xunit.Abstractions;

namespace MSBLOC.Web.Tests.Services
{
    public class JsonWebTokenServiceTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ILogger<JsonWebTokenServiceTests> _logger;

        private static readonly Faker Faker = new Faker();

        public JsonWebTokenServiceTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _logger = TestLogger.Create<JsonWebTokenServiceTests>(testOutputHelper);

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

            var service = new JsonWebTokenService(optionsAccessor, tokenRepository);

            var jwt = await service.CreateTokenAsync(user, githubRepositoryId);

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

            var service = new JsonWebTokenService(optionsAccessor, tokenRepository);
            var jwt = await service.CreateTokenAsync(user, githubRepositoryId);

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

            var service = new JsonWebTokenService(optionsAccessor, tokenRepository);
            var jwt = await service.CreateTokenAsync(user, githubRepositoryId);

            service.Awaiting(async s => await s.ValidateTokenAsync(jwt)).Should().Throw<InvalidOperationException>();
        }
    }
}
