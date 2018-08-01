using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MSBLOC.Core.Model;
using Octokit;

namespace MSBLOC.Core.Interfaces
{
    public interface ICheckRunSubmitter
    {
        /// <summary>
        /// Creates new CheckRun objects in the GitHub API using the build details and other account and repository informations
        /// </summary>
        /// <param name="buildDetails">The build details.</param>
        /// <param name="owner">Owner of the repository.</param>
        /// <param name="name">Name of the repository.</param>
        /// <param name="headSha">The SHA of the commit.</param>
        /// <param name="checkRunName">The name of the check. For example, "code-coverage".</param>
        /// <param name="checkRunTitle">The title of the check.</param>
        /// <param name="checkRunSummary">The summary of the check. This parameter supports Markdown.</param>
        /// <param name="startedAt">The start time of the check.</param>
        /// <param name="completedAt">The completion time of the check.</param>
        /// <returns>An Octokit CheckRun object</returns>
        Task<CheckRun> SubmitCheckRun([NotNull] BuildDetails buildDetails,
            [NotNull] string owner, [NotNull] string name, [NotNull] string headSha,
            [NotNull] string checkRunName, [NotNull] string checkRunTitle, [NotNull] string checkRunSummary,
            DateTimeOffset startedAt, DateTimeOffset completedAt);
    }
}