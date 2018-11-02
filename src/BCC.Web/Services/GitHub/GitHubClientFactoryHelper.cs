using Octokit;
using Octokit.Internal;

namespace BCC.Web.Services.GitHub
{
    public class GitHubClientFactoryHelper
    {
        public static IGitHubClient GitHubClient(string token, string userAgent)
        {
            var productHeaderValue = new ProductHeaderValue(userAgent);
            var credentialStore = new InMemoryCredentialStore(new Credentials(token));
            return new GitHubClient(productHeaderValue, credentialStore);
        }
    }
}