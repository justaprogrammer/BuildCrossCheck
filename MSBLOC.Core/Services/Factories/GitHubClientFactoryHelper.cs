using Octokit;
using Octokit.Internal;

namespace MSBLOC.Core.Services.Factories
{
    public class GitHubClientFactoryHelper
    {
        public static IGitHubClient GitHubClient(string token, string userAgent)
        {
            var productHeaderValue = new ProductHeaderValue(userAgent);
            var credentialStore = new InMemoryCredentialStore(new Credentials(token));
            return new GitHubClient(productHeaderValue, credentialStore);
        }

        public static IGitHubGraphQLClient GraphQLClient(string token, string userAgent)
        {
            return new GitHubGraphQLClient(new Octokit.GraphQL.ProductHeaderValue(userAgent), token);
        }
    }
}