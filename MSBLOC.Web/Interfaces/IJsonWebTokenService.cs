using System;
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using MSBLOC.Web.Models;

namespace MSBLOC.Web.Interfaces
{
    public interface IJsonWebTokenService
    {
        (AccessToken AccessToken, string JsonWebToken) CreateToken(ClaimsPrincipal user, long githubRepositoryId);
        TokenValidationResult ValidateToken(string accessToken);
    }
}