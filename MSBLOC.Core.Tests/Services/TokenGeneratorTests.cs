using Microsoft.Extensions.Logging;
using MSBLOC.Core.Services;
using MSBLOC.Core.Tests.Util;
using NUnit.Framework;
using Shouldly;

namespace MSBLOC.Core.Tests.Services
{
    [TestFixture]
    public class TokenGeneratorTests
    {
        private static readonly ILogger<TokenGeneratorTests> logger = TestLogger.Create<TokenGeneratorTests>();

        [Test]
        public void ShouldGenerateToken()
        {
            var jwtTokenGenerator = new TokenGenerator(new TestPrivateKeySource(), TestLogger.Create<TokenGenerator>());
            var token = jwtTokenGenerator.GetToken();
            token.ShouldNotBeNull();

            logger.LogInformation($"Token: {token}");
        }
    }
}