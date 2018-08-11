using MongoDB.Driver;
using MSBLOC.Infrastructure.Models;
using MSBLOC.Infrastructure.Interfaces;

namespace MSBLOC.Infrastructure.Contexts
{
    public class PersistantDataContext : IPersistantDataContext
    {
        private readonly IMongoDatabase _database;

        public PersistantDataContext(IMongoDatabase database)
        {
            _database = database;
        }

        public IMongoCollection<AccessToken> AccessTokens => _database.GetCollection<AccessToken>("AccessTokens");
    }
}
