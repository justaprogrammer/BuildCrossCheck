using System.Threading.Tasks;
using MSBLOC.Core.Services;
using Octokit;

namespace MSBLOC.Core.Interfaces
{
    public interface IGitHuAppClientFactory
    {
        Task<IGitHubClient> CreateClient(ITokenGenerator tokenGenerator, string login);
        Task<IGitHubGraphQLClient> CreateGraphQLClient(ITokenGenerator tokenGenerator, string login);
    }
}