using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
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
    }
}
