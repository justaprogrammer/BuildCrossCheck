using System.Threading.Tasks;
using MSBLOC.Core.Services;
using Octokit;

namespace MSBLOC.Core.Interfaces
{
    /// <summary>
    /// This factory provides GitHub clients configured for GitHub App Authentication or GitHub App Installation Authentication.
    /// </summary>
    public interface IGitHubAppClientFactory
    {
        /// <summary>
        /// Create a IGitHubClient configured for GitHub App Installation Authentication.
        /// </summary>
        /// <returns>A client</returns>
        Task<IGitHubClient> CreateAppClientForLoginAsync(ITokenGenerator tokenGenerator, string login);

        /// <summary>
        /// Create a IGitHubGraphQLClient configured for GitHub App Installation Authentication.
        /// </summary>
        /// <returns>A graphql client</returns>
        Task<IGitHubGraphQLClient> CreateAppGraphQLClientForLoginAsync(ITokenGenerator tokenGenerator, string login);

        /// <summary>
        /// Create a IGitHubClient configured for GitHub App Authentication.
        /// </summary>
        /// <returns>A client</returns>
        IGitHubClient CreateAppClient(ITokenGenerator tokenGenerator);

        /// <summary>
        /// Create a IGitHubGraphQLClient configured for GitHub App Authentication.
        /// </summary>
        /// <returns>A graphql client</returns>
        IGitHubGraphQLClient CreateAppGraphQLClient(ITokenGenerator tokenGenerator);
    }
}