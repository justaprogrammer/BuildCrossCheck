using System.Threading.Tasks;
using MSBLOC.Core.Services;
using Octokit;

namespace MSBLOC.Core.Interfaces
{
    public interface IGitHubAppClientFactory
    {
        Task<IGitHubClient> CreateAppClientForLoginAsync(ITokenGenerator tokenGenerator, string login);
        Task<IGitHubGraphQLClient> CreateAppGraphQLClientForLoginAsync(ITokenGenerator tokenGenerator, string login);
        IGitHubClient CreateAppClient(ITokenGenerator tokenGenerator);
        IGitHubGraphQLClient CreateAppGraphQLClient(ITokenGenerator tokenGenerator);
    }
}