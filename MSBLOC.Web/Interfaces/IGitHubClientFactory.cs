using System.Threading.Tasks;
using Octokit;

namespace MSBLOC.Web.Interfaces
{
    public interface IGitHubClientFactory
    {
        Task<IGitHubClient> CreateAppClient(string repositoryOwner);
        Task<IGitHubClient> CreateClientForCurrentUser();
    }
}