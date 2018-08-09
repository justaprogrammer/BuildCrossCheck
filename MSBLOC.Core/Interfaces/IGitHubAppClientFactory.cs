using System.Threading.Tasks;
using MSBLOC.Core.Services;
using Octokit;

namespace MSBLOC.Core.Interfaces
{
    public interface IGitHubAppClientFactory
    {
        Task<IGitHubClient> CreateAppClientForLogin(ITokenGenerator tokenGenerator, string login);
        Task<IGitHubGraphQLClient> CreateAppGraphQLClientForLogin(ITokenGenerator tokenGenerator, string login);
        GitHubClient CreateAppClient(ITokenGenerator tokenGenerator);
        IGitHubGraphQLClient CreateAppGraphQLClient(ITokenGenerator tokenGenerator);
    }
}