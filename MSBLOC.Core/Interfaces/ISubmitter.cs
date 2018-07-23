using System.Threading.Tasks;
using MSBLOC.Core.Model;
using Octokit;

namespace MSBLOC.Core.Interfaces
{
    public interface ISubmitter
    {
        Task<CheckRun> SubmitCheckRun(string owner, string name, string headSha,
            string checkRunName, BuildDetails buildDetails, string checkRunTitle, string checkRunSummary, string cloneRoot);
    }
}