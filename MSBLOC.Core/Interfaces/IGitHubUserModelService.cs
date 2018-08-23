using System.Collections.Generic;
using System.Threading.Tasks;
using MSBLOC.Core.Model;
using MSBLOC.Core.Model.GitHub;

namespace MSBLOC.Core.Interfaces
{
    public interface IGitHubUserModelService
    {
        Task<IReadOnlyList<UserInstallation>> GetUserInstallationsAsync();
        Task<UserInstallation> GetUserInstallationAsync(long installationId);
        Task<IReadOnlyList<UserRepository>> GetUserRepositoriesAsync();
        Task<UserRepository> GetUserRepositoryAsync(long repositoryId);
    }
}