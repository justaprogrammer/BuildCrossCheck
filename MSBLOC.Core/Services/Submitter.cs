using System.Threading.Tasks;
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

        public async Task Submit(string owner, string name, string headSha,
            string checkRunName, CheckRunAnnotation[] annotations)
        {
            var jwtToken = TokenGenerator.GetToken();
            var appClient = new GitHubClient(new ProductHeaderValue("MyApp"))
            {
                Credentials = new Credentials(jwtToken, AuthenticationType.Bearer)
            };

            var checkSuite = await appClient.Check.Suite.Create(owner, name, new NewCheckSuite(headSha));
            var checkRun = await appClient.Check.Run.Create(owner, name, new NewCheckRun(checkRunName, headSha));
        }
    }
}