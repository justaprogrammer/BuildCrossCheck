using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
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

        private static Faker _faker;

        public JsonWebTokenServiceTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _logger = TestLogger.Create<JsonWebTokenServiceTests>(testOutputHelper);

            IdentityModelEventSource.ShowPII = true;
        }

        static JsonWebTokenServiceTests()
        {
            _faker = new Faker();
        }

        [Fact]
        public void CreateValidTokenTest()
        {
            var options = new AuthOptions {Secret = new Faker().Random.AlphaNumeric(32)};
            var optionsAccessor = Substitute.For<IOptions<AuthOptions>>();
            optionsAccessor.Value.Returns(options);

            var githubRepositoryId = _faker.Random.Long();
            var email = _faker.Internet.Email();

            var user = Substitute.For<ClaimsPrincipal>();
            user.Claims.Returns(new []
            {
                new Claim(ClaimTypes.Email, email)
            });

            var service = new JsonWebTokenService(optionsAccessor);
            var (accessToken, jwt) = service.CreateToken(user, githubRepositoryId);

            var tokenValidationResult = service.ValidateToken(jwt);

            var jsonWebToken = tokenValidationResult?.SecurityToken as JsonWebToken;

            jsonWebToken.Should().NotBeNull();
            jsonWebToken.Payload.Value<string>(JwtRegisteredClaimNames.Email).Should().Be(email);
            jsonWebToken.Payload.Value<string>(JwtRegisteredClaimNames.Aud).Should().Be("MSBLOC.Api");
            jsonWebToken.Payload.Value<string>(JwtRegisteredClaimNames.Jti).Should().Be(accessToken.Id.ToString());
            DateTimeOffset.FromUnixTimeSeconds(jsonWebToken.Payload.Value<int>(JwtRegisteredClaimNames.Iat)).Should().BeCloseTo(DateTimeOffset.UtcNow, 1000);
            jsonWebToken.Payload.Value<long>("urn:msbloc:repositoryId").Should().Be(githubRepositoryId);
        }

        [Fact]
        public void InvalidTokenVerficationTest()
        {
            var options = new AuthOptions { Secret = new Faker().Random.AlphaNumeric(32) };
            var optionsAccessor = Substitute.For<IOptions<AuthOptions>>();
            optionsAccessor.Value.Returns(options);

            var githubRepositoryId = _faker.Random.Long();
            var email = _faker.Internet.Email();

            var user = Substitute.For<ClaimsPrincipal>();
            user.Claims.Returns(new[]
            {
                new Claim(ClaimTypes.Email, email)
            });

            var service = new JsonWebTokenService(optionsAccessor);
            var (accessToken, jwt) = service.CreateToken(user, githubRepositoryId);

            var tokenValidationResult = service.ValidateToken(jwt);

            var jsonWebToken = tokenValidationResult?.SecurityToken as JsonWebToken;

            jsonWebToken.Should().NotBeNull();
            jsonWebToken.Payload.Value<string>(JwtRegisteredClaimNames.Jti).Should().Be(accessToken.Id.ToString());

            var modifiedToken = jwt + " ";
            service.Invoking(s => s.ValidateToken(modifiedToken)).Should().Throw<SecurityTokenException>();
        }
    }
}
