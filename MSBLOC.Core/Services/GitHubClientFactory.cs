using System.Linq;
using System.Threading.Tasks;
using MSBLOC.Core.Interfaces;
using Octokit;

namespace MSBLOC.Core.Services
{
    public class GitHubClientFactory : IGitHubClientFactory
    {
        public ITokenGenerator TokenGenerator { get; }

        public GitHubClientFactory(ITokenGenerator tokenGenerator)
        {
            TokenGenerator = tokenGenerator;
        }

        public async Task<IGitHubClient> CreateClientForLogin(string login)
        {
            var jwtToken = TokenGenerator.GetToken();

            var appClient = new GitHubClient(new ProductHeaderValue("MSBuildLogOctokitChecker"))
            {
                Credentials = new Credentials(jwtToken, AuthenticationType.Bearer)
            };

            var installations = await appClient.GitHubApps.GetAllInstallationsForCurrent();
            var installation = installations.First(inst => inst.Account.Login.ToLowerInvariant() == login.ToLowerInvariant());

            var response = await appClient.GitHubApps.CreateInstallationToken(installation.Id);

            return new GitHubClient(new ProductHeaderValue("MSBuildLogOctokitChecker-Installation" + installation.Id))
            {
                Credentials = new Credentials(response.Token)
            };
        }
    }
}