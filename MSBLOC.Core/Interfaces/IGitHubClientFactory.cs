using System.Threading.Tasks;
using Octokit;

namespace MSBLOC.Core.Interfaces
{
    public interface IGitHubClientFactory
    {
        Task<IGitHubClient> CreateClientForLogin(string login);
    }
}