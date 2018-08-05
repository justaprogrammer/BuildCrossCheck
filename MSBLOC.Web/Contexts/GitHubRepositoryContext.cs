using MongoDB.Driver;
using MSBLOC.Web.Interfaces;
using MSBLOC.Web.Models;

namespace MSBLOC.Web.Contexts
{
    public class GitHubRepositoryContext : IGitHubRepositoryContext
    {
        private readonly IMongoDatabase _database;

        public GitHubRepositoryContext(IMongoDatabase database)
        {
            _database = database;
        }

        public IMongoCollection<GitHubRepository> Repositories => _database.GetCollection<GitHubRepository>("GitHubRepository");
    }
}
