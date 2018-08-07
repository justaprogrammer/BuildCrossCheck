using MongoDB.Driver;
using MSBLOC.Web.Interfaces;
using MSBLOC.Web.Models;

namespace MSBLOC.Web.Contexts
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
