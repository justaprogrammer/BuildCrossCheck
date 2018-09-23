using System.Collections.Generic;
using System.Threading.Tasks;
using BCC.Core.Interfaces.GitHub;
using Octokit.GraphQL;
using Octokit.GraphQL.Core;

namespace BCC.Core.Services.GitHub
{
    /// <inheritdoc />
    public class GitHubGraphQLClient : IGitHubGraphQLClient
    {
        private readonly Connection _connection;

        public GitHubGraphQLClient(ProductHeaderValue headerValue, string accessToken)
        {
            _connection = new Connection(headerValue, accessToken);
        }

        protected Task<T> Run<T>(IQueryableValue<T> expression)
        {
            return _connection.Run(expression);
        }

        protected Task<IEnumerable<T>> Run<T>(IQueryableList<T> expression)
        {
            return _connection.Run(expression);
        }

        protected Task<T> Run<T>(ICompiledQuery<T> query, Dictionary<string, object> variables = null)
        {
            return _connection.Run(query, variables);
        }
    }
}