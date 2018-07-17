using System;
using System.Threading.Tasks;
using MSBLOC.Core.Model;
using Octokit;

namespace MSBLOC.Core.Interfaces
{
    public interface ISubmitter
    {
        Task<CheckRun> SubmitCheckRun(string owner, string name, string headSha,
            string checkRunName, ParsedBinaryLog parsedBinaryLog, string checkRunTitle, string checkRunSummary,
            DateTimeOffset? completedAt);
    }
}