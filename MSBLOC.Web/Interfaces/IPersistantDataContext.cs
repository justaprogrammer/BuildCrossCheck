using MongoDB.Driver;
using MSBLOC.Web.Models;

namespace MSBLOC.Web.Interfaces
{
    public interface IPersistantDataContext
    {
        IMongoCollection<AccessToken> AccessTokens { get; }
    }
}