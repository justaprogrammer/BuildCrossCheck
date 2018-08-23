using System.Collections.Generic;
using System.Threading.Tasks;
using MSBLOC.Core.Model;
using MSBLOC.Core.Model.GitHub;

namespace MSBLOC.Core.Interfaces
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
        /// Gets an installation by id of the GitHub available to the current user.
        /// </summary>
        /// <returns>A readonly list of Installations</returns>
        Task<Installation> GetInstallationAsync(long installationId);

        /// <summary>
        /// Get all repositories from all installations of the GitHub available to the current user.
        /// </summary>
        /// <returns>A readonly list of repositories</returns>
        Task<IReadOnlyList<Repository>> GetRepositoriesAsync();

        /// <summary>
        /// Gets a repository by id from all installations of the GitHub available to the current user.
        /// </summary>
        /// <param name="repositoryId"></param>
        /// <returns></returns>
        Task<Repository> GetRepositoryAsync(long repositoryId);
    }
}