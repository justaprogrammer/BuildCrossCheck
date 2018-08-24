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