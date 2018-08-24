using System.Threading.Tasks;
using Octokit;

namespace MSBLOC.Core.Interfaces.GitHub
{
    /// <summary>
    /// This factory provides GitHub clients configured for GitHub App User-To-Server Authentication.
    /// </summary>
    public interface IGitHubUserClientFactory
    {
        /// <summary>
        /// Create a IGitHubClient configured for GitHub App User-To-Server Authentication.
        /// </summary>
        /// <returns>A client</returns>
        Task<IGitHubClient> CreateClient();

        /// <summary>
        /// Create a IGitHubGraphQLClient configured for GitHub App User-To-Server Authentication.
        /// </summary>
        /// <returns>A graphql client</returns>
        Task<IGitHubGraphQLClient> CreateGraphQLClient();
    }
}