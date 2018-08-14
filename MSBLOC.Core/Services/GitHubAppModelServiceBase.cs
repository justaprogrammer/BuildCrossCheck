using System.Linq;
using System.Threading.Tasks;
using Octokit;

namespace MSBLOC.Core.Services
{
    public abstract class GitHubAppModelServiceBase
    {
        protected async Task<string[]> GetPullRequestChangedPaths(IGitHubClient gitHubClient, string owner, string name, int number)
        {
            var pullRequestFiles = await gitHubClient.PullRequest.Files(owner, name, number);
            return pullRequestFiles.Select(pullRequestFile => pullRequestFile.FileName).ToArray();
        }
    }
}