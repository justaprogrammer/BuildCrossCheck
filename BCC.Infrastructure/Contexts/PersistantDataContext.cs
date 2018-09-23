using BCC.Infrastructure.Interfaces;
using BCC.Infrastructure.Models;
using MongoDB.Driver;

namespace BCC.Infrastructure.Contexts
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
