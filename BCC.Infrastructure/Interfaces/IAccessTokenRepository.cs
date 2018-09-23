using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BCC.Infrastructure.Models;

namespace BCC.Infrastructure.Interfaces
{
    public interface IAccessTokenRepository : IRepository<AccessToken, Guid>
    {
        Task DeleteAsync(Guid tokenId, IEnumerable<long> repositoryIds);
        Task<IEnumerable<AccessToken>> GetByRepositoryIdsAsync(IEnumerable<long> repositoryIds);
    }
}
