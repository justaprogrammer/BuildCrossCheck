using Microsoft.Extensions.Logging;
using MSBLOC.Core.Services;
using MSBLOC.Core.Tests.Util;
using Shouldly;
using Xunit;

namespace MSBLOC.Core.Tests.Services
{
    public class TokenGeneratorTests
    {
        private static readonly ILogger<TokenGeneratorTests> logger = TestLogger.Create<TokenGeneratorTests>();

        [Fact]
        public void ShouldGenerateToken()
        {
            var jwtTokenGenerator = new TokenGenerator(new TestPrivateKeySource(), TestLogger.Create<TokenGenerator>());
            var token = jwtTokenGenerator.GetToken();
            token.ShouldNotBeNull();

            logger.LogInformation($"Token: {token}");
        }
    }
}