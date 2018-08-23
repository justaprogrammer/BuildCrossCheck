using System;
using System.Threading.Tasks;
using MSBLOC.Core.Model;
using MSBLOC.Core.Model.GitHub;
using MSBLOC.Core.Model.LogAnalyzer;

namespace MSBLOC.Core.Interfaces
{
    /// <summary>
    /// This service makes calls to the GitHub Api with GitHub App Installation Authentication.
    /// </summary>
    public interface IGitHubAppModelService
    {
        /// <summary>
        /// Creates a CheckRun in the GitHub Api.
        /// </summary>
        /// <param name="owner">The name of the repository owner.</param>
        /// <param name="repository">The name of the repository.</param>
        /// <param name="sha">The sha we are creating this CheckRun for.</param>
        /// <param name="checkRunName">The name of the CheckRun.</param>
        /// <param name="checkRunTitle">The title of the CheckRun.</param>
        /// <param name="checkRunSummary">The summary of the CheckRun.</param>
        /// <param name="checkRunIsSuccess">If the CheckRun is a success.</param>
        /// <param name="annotations">Array of Annotations for the CheckRun.</param>
        /// <param name="startedAt">The time when processing started</param>
        /// <param name="completedAt">The time when processing finished</param>
        /// <returns></returns>
        Task<CheckRun> CreateCheckRunAsync(string owner, string repository, string sha,
            string checkRunName, string checkRunTitle,
            string checkRunSummary, bool checkRunIsSuccess, Annotation[] annotations, DateTimeOffset? startedAt,
            DateTimeOffset? completedAt);

        /// <summary>
        /// Updates a CheckRun in the GitHub Api.
        /// </summary>
        /// <param name="checkRunId">The id of the CheckRun being updated.</param>
        /// <param name="owner">The name of the repository owner.</param>
        /// <param name="repository">The name of the repository</param>
        /// <param name="sha">The sha we are creating this CheckRun for.</param>
        /// <param name="checkRunTitle">The title of the CheckRun.</param>
        /// <param name="checkRunSummary">The summary of the CheckRun.</param>
        /// <param name="annotations">Array of Annotations for the CheckRun.</param>
        /// <param name="startedAt">The time when processing started</param>
        /// <param name="completedAt">The time when processing finished</param>
        /// <returns></returns>
        Task UpdateCheckRunAsync(long checkRunId, string owner, string repository,
            string sha, string checkRunTitle, string checkRunSummary, Annotation[] annotations,
            DateTimeOffset? startedAt, DateTimeOffset? completedAt);

        Task<string> GetRepositoryFileAsync(string owner, string repository, string path, string reference);

        Task<LogAnalyzerConfiguration> GetLogAnalyzerConfigurationAsync(string owner, string repository, string reference);
    }
}