using Octokit;

namespace MSBLOC.Core.Interfaces
{
    /// <summary>
    /// This factory provides GitHub clients configured for with a token for Authentication.
    /// </summary>
    public interface IGitHubClientFactory
    {
        /// <summary>
        /// Create a IGitHubClient configured with a token for Authentication.
        /// </summary>
        /// <returns>A client</returns>
        IGitHubClient CreateClient(string token);

        /// <summary>
        /// Create a IGitHubGraphQLClient configured with a token for Authentication.
        /// </summary>
        /// <returns>A graphql client</returns>
        IGitHubGraphQLClient CreateGraphQLClient(string token);
    }
}