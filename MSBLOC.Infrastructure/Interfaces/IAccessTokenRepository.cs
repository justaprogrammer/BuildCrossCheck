using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MSBLOC.Infrastructure.Models;

namespace MSBLOC.Infrastructure.Interfaces
{
    public interface IAccessTokenRepository : IRepository<AccessToken, Guid>
    {
        Task DeleteAsync(Guid tokenId, IEnumerable<long> repositoryIds);
        Task<IEnumerable<AccessToken>> GetByRepositoryIdsAsync(IEnumerable<long> repositoryIds);
    }
}
