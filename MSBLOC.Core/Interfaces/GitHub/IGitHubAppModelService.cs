using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MSBLOC.Core.Model.GitHub;
using MSBLOC.Core.Model.LogAnalyzer;

namespace MSBLOC.Core.Interfaces.GitHub
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
        /// <param name="headSha">The sha we are creating this CheckRun for.</param>
        /// <param name="name">The name of the CheckRun.</param>
        /// <param name="title">The title of the CheckRun.</param>
        /// <param name="summary">The summary of the CheckRun.</param>
        /// <param name="success">If the CheckRun is a success.</param>
        /// <param name="annotations">Array of Annotations for the CheckRun.</param>
        /// <param name="startedAt">The time when processing started</param>
        /// <param name="completedAt">The time when processing finished</param>
        /// <returns></returns>
        Task<Model.GitHub.CheckRun> SubmitCheckRunAsync([NotNull] string owner,
            [NotNull] string repository, [NotNull] string headSha, [NotNull] string name,
            [NotNull] string title, [CanBeNull] string summary, bool success,
            [CanBeNull] Annotation[] annotations, DateTimeOffset startedAt, DateTimeOffset completedAt);
    }
}