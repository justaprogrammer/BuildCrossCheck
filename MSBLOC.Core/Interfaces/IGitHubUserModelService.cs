using System.Collections.Generic;
using System.Threading.Tasks;
using MSBLOC.Core.Model;

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