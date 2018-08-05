using System.Threading.Tasks;
using MSBLOC.Core.Services;
using Octokit;

namespace MSBLOC.Core.Interfaces
{
    public interface IGitHubClientFactory
    {
        Task<IGitHubClient> CreateAppClient(string login);
        Task<IGitHubGraphQLClient> CreateAppGraphQLClient(string login);
        IGitHubClient CreateClient(string token);
        IGitHubGraphQLClient CreateGraphQLClient(string token);
    }
}