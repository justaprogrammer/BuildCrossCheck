using System.Collections.Generic;
using System.Threading.Tasks;
using MSBLOC.Core.Model.GitHub;

namespace MSBLOC.Core.Interfaces
{
    /// <summary>
    /// This service makes calls to the GitHub GraphQL Api.
    /// </summary>
    public interface IGitHubGraphQLClient
    {
        /// <summary>
        /// Gets a list of commits that are contained by a pull request
        /// </summary>
        /// <param name="owner">Name of the owner.</param>
        /// <param name="repository">Name of the repository.</param>
        /// <param name="pullRequest">Number of the pull request.</param>
        /// <returns>A readonly list of commit details</returns>
        Task<IReadOnlyList<CommitDetails>> GetCommitDetailsByPullRequestIdAsync(string owner, string repository,
            int pullRequest);
    }
}