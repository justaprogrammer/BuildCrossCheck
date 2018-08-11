using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.JsonWebTokens;
using MSBLOC.Infrastructure.Models;

namespace MSBLOC.Web.Interfaces
{
    public interface IAccessTokenService
    {
        Task<string> CreateTokenAsync(long githubRepositoryId);
        Task<JsonWebToken> ValidateTokenAsync(string accessToken);
        Task RevokeTokenAsync(Guid tokenId);
        Task<IEnumerable<AccessToken>> GetTokensForUserRepositoriesAsync();
    }
}