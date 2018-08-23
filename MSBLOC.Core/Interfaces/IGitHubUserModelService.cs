using System.Collections.Generic;
using System.Threading.Tasks;
using MSBLOC.Core.Model;
using MSBLOC.Core.Model.GitHub;

namespace MSBLOC.Core.Interfaces
{
    public interface IGitHubUserModelService
    {
        Task<IReadOnlyList<Installation>> GetUserInstallationsAsync();
        Task<Installation> GetUserInstallationAsync(long installationId);
        Task<IReadOnlyList<Repository>> GetUserRepositoriesAsync();
        Task<Repository> GetUserRepositoryAsync(long repositoryId);
    }
}