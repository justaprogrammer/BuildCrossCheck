using FluentAssertions;
using Microsoft.Extensions.Logging;
using MSBLOC.Core.Services;
using MSBLOC.Core.Tests.Util;
using Xunit;
using Xunit.Abstractions;

namespace MSBLOC.Core.Tests.Services
{
    public class TokenGeneratorTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ILogger<TokenGeneratorTests> _logger;

        public TokenGeneratorTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _logger = TestLogger.Create<TokenGeneratorTests>(testOutputHelper);
        }

        [Fact]
        public void ShouldGenerateToken()
        {
            var jwtTokenGenerator = new TokenGenerator(1, new TestPrivateKeySource(), TestLogger.Create<TokenGenerator>(_testOutputHelper));
            var token = jwtTokenGenerator.GetToken();
            token.Should().NotBeNull();

            _logger.LogInformation($"Token: {token}");
        }
    }
}