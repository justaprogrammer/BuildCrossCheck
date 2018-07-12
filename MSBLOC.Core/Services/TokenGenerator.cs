using System.IO;
using GitHubJwt;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MSBLOC.Core.Interfaces;

namespace MSBLOC.Core.Services
{
    public class TokenGenerator : ITokenGenerator
    {
        private IPrivateKeySource _privateKeySource;
        private  ILogger<TokenGenerator> Logger { get; }

        public TokenGenerator(IPrivateKeySource privateKeySource, ILogger<TokenGenerator> logger = null)
        {
            _privateKeySource = privateKeySource;
            Logger = logger ?? new NullLogger<TokenGenerator>();
        }

        public string GetToken()
        {
            var generator = new GitHubJwtFactory(
                _privateKeySource, 
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