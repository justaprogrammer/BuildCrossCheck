using MongoDB.Driver;
using MSBLOC.Web.Models;

namespace MSBLOC.Web.Interfaces
{
    public interface IGitHubRepositoryContext
    {
        IMongoCollection<GitHubRepository> Repositories { get; }
    }
}