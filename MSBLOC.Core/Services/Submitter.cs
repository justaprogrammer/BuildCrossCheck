using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MSBLOC.Core.Interfaces;
using Octokit;

namespace MSBLOC.Core.Services
{
    public class Submitter
    {
        private ITokenGenerator TokenGenerator { get; }
        private ILogger<Submitter> Logger { get; }

        public Submitter(ITokenGenerator tokenGenerator, ILogger<Submitter> logger = null)
        {
            TokenGenerator = tokenGenerator;
            Logger = logger ?? new NullLogger<Submitter>();
        }

        public void Submit()
        {
            var jwtToken = TokenGenerator.GetToken();
            var appClient = new GitHubClient(new ProductHeaderValue("MyApp"))
            {
                Credentials = new Credentials(jwtToken, AuthenticationType.Bearer)
            };
        }
    }
}