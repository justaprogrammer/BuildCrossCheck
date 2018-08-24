using System;
using System.Linq;
using System.Threading.Tasks;
using Octokit;

namespace MSBLOC.Core.Services.GitHub
{
    /// <summary>
    /// This service makes calls to the GitHub Api with any authenticated GitHub Client.
    /// </summary>
    public abstract class GitHubAppModelServiceBase
    {
        [Obsolete("This is more of an example than anything else")]
        protected async Task<string[]> GetPullRequestChangedPaths(IGitHubClient gitHubClient, string owner, string name, int number)
        {
            var pullRequestFiles = await gitHubClient.PullRequest.Files(owner, name, number);
            return pullRequestFiles.Select(pullRequestFile => pullRequestFile.FileName).ToArray();
        }
    }
}