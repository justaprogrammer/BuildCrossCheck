using Octokit;
using IGitHubClientFactory = BCC.Web.Interfaces.GitHub.IGitHubClientFactory;

namespace BCC.Web.Services.GitHub
{
    /// <inheritdoc />
    public class GitHubClientFactory : IGitHubClientFactory
    {
        public const string UserAgent = "BuildCrossCheck";

        /// <inheritdoc />
        public IGitHubClient CreateClient(string token)
        {
            return GitHubClientFactoryHelper.GitHubClient(token, UserAgent);
        }
    }
}