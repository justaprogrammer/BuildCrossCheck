using MSBLOC.Core.Interfaces;
using Octokit.GraphQL;

namespace MSBLOC.Core.Services
{
    public class GitHubGraphQLClient: IGitHubGraphQLClient
    {
        private Connection _connection;

        public GitHubGraphQLClient(ProductHeaderValue headerValue, string accessToken)
        {
            _connection = new Connection(headerValue, accessToken);
        }
    }
}