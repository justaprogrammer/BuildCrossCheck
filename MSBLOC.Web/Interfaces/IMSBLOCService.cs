using System.Threading.Tasks;
using MSBLOC.Web.Models;
using Octokit;

namespace MSBLOC.Web.Interfaces
{
    public interface IMSBLOCService
    {
        /// <summary>
        /// Submits the provided SubmissionData to GitHub
        /// </summary>
        /// <param name="repositoryOwner">The owner of the repository.</param>
        /// <param name="repositoryName">The name of the repository.</param>
        /// <param name="submissionData">The submission data.</param>
        /// <returns>An Octokit CheckRun object</returns>
        Task<CheckRun> SubmitAsync(string repositoryOwner, string repositoryName, SubmissionData submissionData);
    }
}