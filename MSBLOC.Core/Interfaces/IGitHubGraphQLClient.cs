using System.Collections.Generic;
using System.Threading.Tasks;
using MSBLOC.Core.Model;
using Octokit.GraphQL;
using Octokit.GraphQL.Core;

namespace MSBLOC.Core.Interfaces
{
    public interface IGitHubGraphQLClient
    {
        Task<IEnumerable<CommitDetails>> GetCommitDetailsByPullRequestId(string owner, string name,
            int pullRequest);
    }
}