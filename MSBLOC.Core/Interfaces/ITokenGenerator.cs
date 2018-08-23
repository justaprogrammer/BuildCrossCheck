namespace MSBLOC.Core.Interfaces
{
    /// <summary>
    /// This service provides functionality to create a GitHub App token for authentrication.
    /// </summary>
    public interface ITokenGenerator
    {
        /// <summary>
        /// Creates a token.
        /// </summary>
        /// <param name="expirationSeconds">Time until the token expires (maximum of 1 hour).</param>
        /// <returns>A token</returns>
        string GetToken(int expirationSeconds = 600);
    }
}