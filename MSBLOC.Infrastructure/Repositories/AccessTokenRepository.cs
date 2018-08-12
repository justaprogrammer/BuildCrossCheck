using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using MSBLOC.Infrastructure.Interfaces;
using MSBLOC.Infrastructure.Models;

namespace MSBLOC.Infrastructure.Repositories
{
    public class AccessTokenRepository : MongoDbRepository<AccessToken, Guid>, IAccessTokenRepository
    {
        public AccessTokenRepository(IMongoCollection<AccessToken> entities) : base(entities, token => token.Id)
        {
        }

        public Task<IEnumerable<AccessToken>> GetByRepositoryIdsAsync(IEnumerable<long> repositoryIds)
        { 
            var filter = Builders<AccessToken>.Filter;
            var filterDefinition = filter.In(token => token.GitHubRepositoryId, repositoryIds);

            return GetAllAsync(filterDefinition);
        }

        public Task DeleteAsync(Guid tokenId, IEnumerable<long> repositoryIds)
        {
            var filter = Builders<AccessToken>.Filter;
            var filterDefinition = filter.And(
                filter.Eq(token => token.Id, tokenId),
                filter.In(token => token.GitHubRepositoryId, repositoryIds));

            return DeleteAsync(filterDefinition);
        }
    }
}
