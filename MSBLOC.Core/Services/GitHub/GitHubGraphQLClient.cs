using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MSBLOC.Core.Interfaces.GitHub;
using MSBLOC.Core.Model.GitHub;
using Nito.AsyncEx;
using Octokit.GraphQL;
using Octokit.GraphQL.Core;

namespace MSBLOC.Core.Services.GitHub
{
    /// <inheritdoc />
    public class GitHubGraphQLClient : IGitHubGraphQLClient
    {
        private static readonly AsyncLazy<ICompiledQuery<IEnumerable<CommitDetail>>> CommitDetailsByPullRequestId =
            new AsyncLazy<ICompiledQuery<IEnumerable<CommitDetail>>>(() =>
                Task.FromResult(new Query()
                    .Repository(Variable.Var("owner"), Variable.Var("name"))
                    .PullRequest(Variable.Var("pullRequest"))
                    .Commits(null, null, null, null)
                    .AllPages()
                    .Select(commit => new CommitDetail
                    {
                        Oid = commit.Commit.Oid,
                        ChangedFiles = commit.Commit.ChangedFiles
                    })
                    .Compile()));

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

        /// <inheritdoc />
        public async Task<IReadOnlyList<CommitDetail>> GetCommitDetailsByPullRequestIdAsync(string owner, string repository,
            int pullRequest)
        {
            var query = await CommitDetailsByPullRequestId;

            var commitDetails = await _connection.Run(query, new Dictionary<string, object>()
            {
                {nameof(owner), owner},
                {nameof(repository), repository},
                {nameof(pullRequest), pullRequest}
            });

            return commitDetails.ToArray();
        }
    }
}