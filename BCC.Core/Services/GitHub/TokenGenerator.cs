using BCC.Core.Interfaces.GitHub;
using GitHubJwt;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace BCC.Core.Services.GitHub
{
    /// <inheritdoc />
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

        /// <inheritdoc />
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