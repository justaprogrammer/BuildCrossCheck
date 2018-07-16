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
                    AppIntegrationId = 1,
                    ExpirationSeconds = 600
                }
            );

            return generator.CreateEncodedJwtToken();
        }
    }
}