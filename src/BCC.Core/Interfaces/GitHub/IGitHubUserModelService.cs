using System.Collections.Generic;
using System.Threading.Tasks;
using BCC.Core.Model.GitHub;

namespace BCC.Core.Interfaces.GitHub
{
    /// <summary>
    /// This service makes calls to the GitHub Api using the GitHub App User-To-Server Authentication.
    /// </summary>
    public interface IGitHubUserModelService
    {
        /// <summary>
        /// Get all installations of the GitHub available to the current user.
        /// </summary>
        /// <returns>A readonly list of installations</returns>
        Task<IReadOnlyList<Installation>> GetInstallationsAsync();

        /// <summary>
        /// Get all repositories from all installations of the GitHub available to the current user.
        /// </summary>
        /// <returns>A readonly list of repositories</returns>
        Task<IReadOnlyList<Repository>> GetRepositoriesAsync();

        /// <summary>
        /// Gets a repository by id from all installations of the GitHub available to the current user.
        /// </summary>
        /// <param name="repositoryId">The repository id.</param>
        /// <returns></returns>
        Task<Repository> GetRepositoryAsync(long repositoryId);
    }
}