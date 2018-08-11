using MongoDB.Driver;
using MSBLOC.Infrastructure.Models;

namespace MSBLOC.Infrastructure.Interfaces
{
    public interface IPersistantDataContext
    {
        IMongoCollection<AccessToken> AccessTokens { get; }
    }
}