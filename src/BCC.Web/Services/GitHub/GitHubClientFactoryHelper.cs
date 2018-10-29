using BCC.Core.Interfaces.GitHub;
using Octokit;
using Octokit.Internal;
using IGitHubGraphQLClient = BCC.Web.Interfaces.GitHub.IGitHubGraphQLClient;

namespace BCC.Web.Services.GitHub
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