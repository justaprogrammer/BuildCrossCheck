using System.Collections.Generic;
using System.Threading.Tasks;
using MSBLOC.Core.Model;

namespace MSBLOC.Core.Interfaces
{
    public interface IGitHubUserModelService
    {
        Task<IReadOnlyList<UserInstallation>> GetUserInstallations();
        Task<UserInstallation> GetUserInstallation(long installationId);
    }
}