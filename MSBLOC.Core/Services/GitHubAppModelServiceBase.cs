using System.Linq;
using System.Threading.Tasks;
using Octokit;

namespace MSBLOC.Core.Services
{
    public abstract class GitHubAppModelServiceBase
    {
        protected async Task<string[]> GetPullRequestChangedPathsAsync(IGitHubClient gitHubClient, string owner, string repository, int number)
        {
            var pullRequestFiles = await gitHubClient.PullRequest.Files(owner, repository, number);
            return pullRequestFiles.Select(pullRequestFile => pullRequestFile.FileName).ToArray();
        }

        protected async Task<string> GetRepositoryFileAsync(IGitHubClient gitHubClient, string owner, string repository,
            string path, string reference)
        {
            try
            {
                var repositoryContents = await gitHubClient.Repository.Content.GetAllContentsByRef(owner, repository, path, reference);
                return repositoryContents.FirstOrDefault()?.Content;
            }
            catch (System.Exception ex)
            {
                return null;
            }
        }
    }
}