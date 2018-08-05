using MSBLOC.Core.Services;
using Octokit;
using System.Threading.Tasks;

namespace MSBLOC.Web.Interfaces
{
    public interface IGitHubUserClientFactory
    {
        Task<IGitHubClient> CreateClient();
        Task<IGitHubGraphQLClient> CreateGraphQLClient();
    }
}