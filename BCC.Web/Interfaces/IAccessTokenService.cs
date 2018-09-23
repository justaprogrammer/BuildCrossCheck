namespace BCC.Web.Interfaces
{
    public interface IAccessTokenService
    {
        Task<string> CreateTokenAsync(long githubRepositoryId);
        Task<JsonWebToken> ValidateTokenAsync(string accessToken);
        Task RevokeTokenAsync(Guid tokenId);
        Task<IEnumerable<AccessToken>> GetTokensForUserRepositoriesAsync();
    }
}