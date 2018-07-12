using System.IO;
using GitHubJwt;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MSBLOC.Core.Interfaces;

namespace MSBLOC.Core.Services
{
    public class TokenGenerator : ITokenGenerator
    {
        private  ILogger<TokenGenerator> Logger { get; }

        public TokenGenerator(ILogger<TokenGenerator> logger = null)
        {
            Logger = logger ?? new NullLogger<TokenGenerator>();
        }

        public string GetToken()
        {
            var generator = new GitHubJwtFactory(
                new FilePrivateKeySource("Resources\\private-key.pem"), 
                new GitHubJwtFactoryOptions
                {
                    AppIntegrationId = 1, // The GitHub App Id
                    ExpirationSeconds = 600 // 10 minutes is the maximum time allowed
                }
            );

            return generator.CreateEncodedJwtToken();
        }
    }
}