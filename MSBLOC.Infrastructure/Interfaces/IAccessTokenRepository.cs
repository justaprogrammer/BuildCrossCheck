using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MSBLOC.Infrastructure.Models;

namespace MSBLOC.Infrastructure.Interfaces
{
    public interface IAccessTokenRepository
    {
        Task DeleteAsync(Guid tokenId, IEnumerable<long> repositoryIds);
        Task<AccessToken> GetAsync(Guid tokenId);
        Task<IEnumerable<AccessToken>> GetByRepositoryIds(IEnumerable<long> repositoryIds);
        Task AddAsync(AccessToken accessToken);
    }
}
