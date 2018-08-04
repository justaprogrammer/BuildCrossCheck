using System.Threading.Tasks;
using Octokit;
using Connection = Octokit.GraphQL.Connection;

namespace MSBLOC.Core.Interfaces
{
    public interface IGitHubClientFactory
    {
        Task<IGitHubClient> CreateGitHubAppClientForLogin(string login);
        IGitHubClient CreateClientForToken(string accessToken);
        Connection CreateGraphQlConnectionForToken(string accessToken);
    }
}