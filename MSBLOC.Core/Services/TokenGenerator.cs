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
        private int _appIntegrationId;
        private  ILogger<TokenGenerator> Logger { get; }

        public TokenGenerator(int appIntegrationId, IPrivateKeySource privateKeySource,
            ILogger<TokenGenerator> logger = null)
        {
            _privateKeySource = privateKeySource;
            Logger = logger ?? new NullLogger<TokenGenerator>();
            _appIntegrationId = appIntegrationId;
        }

        public string GetToken(int expirationSeconds = 600)
        {
            var readToEnd = _privateKeySource.GetPrivateKeyReader().ReadToEnd();

            var generator = new GitHubJwtFactory(
                _privateKeySource, 
                new GitHubJwtFactoryOptions
                {
                    AppIntegrationId = _appIntegrationId,
                    ExpirationSeconds = expirationSeconds
                }
            );

            return generator.CreateEncodedJwtToken();
        }
    }
}