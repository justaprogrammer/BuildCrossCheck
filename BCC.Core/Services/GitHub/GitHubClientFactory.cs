using BCC.Core.Interfaces.GitHub;
using Octokit;

namespace BCC.Core.Services.GitHub
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

        /// <inheritdoc />
        public IGitHubGraphQLClient CreateGraphQLClient(string token)
        {
            return GitHubClientFactoryHelper.GraphQLClient(token, UserAgent);
        }
    }
}