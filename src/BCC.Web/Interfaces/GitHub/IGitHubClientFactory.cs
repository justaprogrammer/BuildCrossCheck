using Octokit;

namespace BCC.Web.Interfaces.GitHub
{
    /// <summary>
    /// This factory provides GitHub clients configured for with a token for Authentication.
    /// </summary>
    public interface IGitHubClientFactory
    {
        /// <summary>
        /// Create a IGitHubClient configured with a token for Authentication.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>A client</returns>
        IGitHubClient CreateClient(string token);
    }
}