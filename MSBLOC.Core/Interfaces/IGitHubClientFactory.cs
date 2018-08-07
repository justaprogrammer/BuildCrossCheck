using MSBLOC.Core.Services;
using Octokit;

namespace MSBLOC.Core.Interfaces
{
    public interface IGitHubClientFactory
    {
        IGitHubClient CreateClient(string token);
        IGitHubGraphQLClient CreateGraphQLClient(string token);
    }
}