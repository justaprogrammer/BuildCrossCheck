using System;
using System.Threading.Tasks;
using MSBLOC.Core.Model;
using MSBLOC.Core.Model.GitHub;
using MSBLOC.Core.Model.LogAnalyzer;

namespace MSBLOC.Core.Interfaces
{
    public interface IGitHubAppModelService
    {
        Task GetPullRequestChangedPathsAsync(string repoOwner, string repoName, int number);

        Task<CheckRun> CreateCheckRunAsync(string repoOwner, string repoName, string headSha,
            string checkRunName, string checkRunTitle,
            string checkRunSummary, bool checkRunIsSuccess, Annotation[] annotations, DateTimeOffset? startedAt,
            DateTimeOffset? completedAt);

        Task UpdateCheckRunAsync(long checkRunId, string repoOwner, string repoName,
            string headSha, string checkRunTitle, string checkRunSummary, Annotation[] annotations,
            DateTimeOffset? startedAt, DateTimeOffset? completedAt);

        Task<string> GetRepositoryFileAsync(string owner, string repository, string path, string reference);

        Task<LogAnalyzerConfiguration> GetLogAnalyzerConfigurationAsync(string owner, string repository, string reference);
    }
}