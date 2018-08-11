using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.JsonWebTokens;
using MSBLOC.Infrastructure.Models;

namespace MSBLOC.Web.Interfaces
{
    public interface IJsonWebTokenService
    {
        Task<string> CreateTokenAsync(ClaimsPrincipal user, long githubRepositoryId);
        Task<JsonWebToken> ValidateTokenAsync(string accessToken);
    }
}