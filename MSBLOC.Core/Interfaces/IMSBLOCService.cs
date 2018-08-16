using System.Threading.Tasks;
using MSBLOC.Core.Model;

namespace MSBLOC.Core.Interfaces
{
    public interface IMSBLOCService
    {
        /// <summary>
        /// Submits the provided SubmissionData to GitHub
        /// </summary>
        /// <param name="repoOwner"></param>
        /// <param name="repoName"></param>
        /// <param name="sha"></param>
        /// <param name="cloneRoot"></param>
        /// <param name="resourcePath"></param>
        /// <returns>A CheckRun object</returns>
        Task<CheckRun> SubmitAsync(string repoOwner, string repoName, string sha, string cloneRoot, string resourcePath);
    }
}