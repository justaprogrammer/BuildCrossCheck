using MSBLOC.Core.Interfaces;
using Octokit;

namespace MSBLOC.Core.Services.Factories
{
    public class GitHubClientFactory : IGitHubClientFactory
    {
        public const string UserAgent = "MSBuildLogOctokitChecker";

        public IGitHubClient CreateClient(string token)
        {
            return GitHubClientFactoryHelper.GitHubClient(token, UserAgent);
        }

        public IGitHubGraphQLClient CreateGraphQLClient(string token)
        {
            return GitHubClientFactoryHelper.GraphQLClient(token, UserAgent);
        }
    }
}