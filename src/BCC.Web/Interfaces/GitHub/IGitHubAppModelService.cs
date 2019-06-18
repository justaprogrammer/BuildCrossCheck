using System;
using System.Threading.Tasks;
using BCC.Core.Model.CheckRunSubmission;
using BCC.Web.Models.GitHub;
using JetBrains.Annotations;

namespace BCC.Web.Interfaces.GitHub
{
    /// <summary>
    /// This service makes calls to the GitHub Api with GitHub App Installation Authentication.
    /// </summary>
    public interface IGitHubAppModelService
    {
        Task<string> GetRepositoryFileAsync(string owner, string repository, string path, string reference);

        /// <summary>
        /// Creates a CheckRun in the GitHub Api.
        /// </summary>
        /// <param name="owner">The name of the repository owner.</param>
        /// <param name="repository">The name of the repository.</param>
        /// <param name="sha">The sha we are creating this CheckRun for.</param>
        /// <param name="checkRun"></param>
        /// <param name="annotations">Array of Annotations for the CheckRun.</param>
        /// <returns></returns>
        Task<CheckRun> SubmitCheckRunAsync([NotNull] string owner, [NotNull] string repository, [NotNull] string sha, CreateCheckRun checkRun, Annotation[] annotations);

        Task<string[]> GetPullRequestFiles(string owner, string repository, int number);
    }
}