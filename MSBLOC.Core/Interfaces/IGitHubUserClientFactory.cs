using System.Threading.Tasks;
using MSBLOC.Core.Services;
using Octokit;

namespace MSBLOC.Core.Interfaces
{
    public interface IGitHubUserClientFactory
    {
        Task<IGitHubClient> CreateClient();
        Task<IGitHubGraphQLClient> CreateGraphQLClient();
    }
}