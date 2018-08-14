using System;
using System.Threading.Tasks;
using MSBLOC.Core.Model;

namespace MSBLOC.Core.Interfaces
{
    public interface IGitHubAppModelService
    {
        Task GetPullRequestChangedPaths(string repoOwner, string repoName, int number);

        Task<CheckRun> CreateCheckRun(string repoOwner, string repoName, string headSha,
            string checkRunName, string checkRunTitle,
            string checkRunSummary, Annotation[] annotations, DateTimeOffset? startedAt, DateTimeOffset? completedAt);

        Task UpdateCheckRun(long checkRunId, string repoOwner, string repoName,
            string headSha, string checkRunTitle, string checkRunSummary, Annotation[] annotations,
            DateTimeOffset? startedAt, DateTimeOffset? completedAt);
    }
}