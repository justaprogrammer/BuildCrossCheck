using BCC.Infrastructure.Models;
using MongoDB.Driver;

namespace BCC.Infrastructure.Interfaces
{
    public interface IPersistantDataContext
    {
        IMongoCollection<AccessToken> AccessTokens { get; }
    }
}